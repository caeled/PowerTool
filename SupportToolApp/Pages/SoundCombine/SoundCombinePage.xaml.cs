namespace SupportToolApp.Pages.SoundCombine;

public partial class SoundCombinePage : ContentPage
{
    private readonly SoundCombineViewModel _vm;

    public SoundCombinePage()
    {
        InitializeComponent();
        _vm = new SoundCombineViewModel();
        _vm.SwitchSimpleCommand = new Command(() => SetMode(simple: true));
        _vm.SwitchBatchCommand  = new Command(() => SetMode(simple: false));
        BindingContext = _vm;
        SetMode(simple: true);
    }

    private void SetMode(bool simple)
    {
        _vm.ModeSimple = simple;
        _vm.ModeBatch  = !simple;
        BtnSimple.BackgroundColor = simple ? Color.FromArgb("#0078D4") : Colors.White;
        BtnSimple.TextColor       = simple ? Colors.White : Colors.Black;
        BtnBatch.BackgroundColor  = simple ? Colors.White : Color.FromArgb("#0078D4");
        BtnBatch.TextColor        = simple ? Colors.Black : Colors.White;
    }
}
