# IT Support Tool — Dev Notes

---

## Remote Access — Firewall / WinRM Issues on PowellServer
**Date:** 2026-06-26  
**Status:** TODO — revisit when on-network or VPN

### Event Viewer (Remote)
Error: `Computer 'POWELLSERVER' cannot be connected.`  
Cause: Windows Firewall blocking DCOM/RPC inbound on the target.  
Fix needed on target machine:
- Enable **COM+ Network Access (DCOM-In)** firewall rule
- Enable all rules in the **Remote Event Log Management** group
- Or push via Group Policy: Windows Firewall with Advanced Security → inbound rules

### Computer Management (Remote)
Same root cause — DCOM/RPC blocked by firewall.  
Same fix applies.

### Run Remote Command (PowerShell Invoke-Command)
Error: `WinRM client cannot process the request... ServerNotTrusted, PSSessionStateBroken`  
Cause: PowellServer not in WinRM TrustedHosts and/or not domain-joined to same domain as client.  
Fix options:
1. Add to TrustedHosts: `winrm set winrm/config/client '@{TrustedHosts="PowellServer"}'`
2. Use HTTPS transport instead of default HTTP
3. If domain-joined on both ends, Kerberos should handle it automatically

### Notes
- The app code is working correctly — it launches the tools and captures output as expected
- These are infrastructure/network issues, not application bugs
- RDP to the same server works fine (different protocol/port)

---
