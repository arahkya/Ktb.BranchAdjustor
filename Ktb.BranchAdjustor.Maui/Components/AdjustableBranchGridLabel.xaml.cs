using System.Windows.Input;
using Ktb.BranchAdjustor.Maui.Models;

namespace Ktb.BranchAdjustor.Maui.Components;

public partial class AdjustableBranchGridLabel : ContentView
{
    public int Index
    {
        get => (int)GetValue(IndexProperty);
        set => SetValue(IndexProperty, value);
    }

    public static readonly BindableProperty IndexProperty = BindableProperty.Create(
        nameof(Index),
        typeof(int),
        typeof(AdjustableBranchGridLabel)
    );

    public int MaxLimit
    {
        get => (int)GetValue(MaxLimitProperty);
        set => SetValue(MaxLimitProperty, value);
    }

    public static readonly BindableProperty MaxLimitProperty = BindableProperty.Create(
        nameof(MaxLimit),
        typeof(int),
        typeof(AdjustableBranchGridLabel)
    );

    public string Position
    {
        get => (string)GetValue(PositionProperty);
        set => SetValue(PositionProperty, value);
    }

    public static readonly BindableProperty PositionProperty = BindableProperty.Create(
        nameof(Position),
        typeof(string),
        typeof(AdjustableBranchGridLabel)
    );

    public ICommand AdjustCommand
    {
        get => (ICommand)GetValue(AdjustCommandProperty);
        set => SetValue(AdjustCommandProperty, value);
    }

    public static readonly BindableProperty AdjustCommandProperty = BindableProperty.Create(
        nameof(AdjustCommand),
        typeof(ICommand),
        typeof(AdjustableBranchGridLabel),
        null
    );

    public int Branch
    {
        get => (int)GetValue(BranchProperty);
        set => SetValue(BranchProperty, value);
    }
    public static readonly BindableProperty BranchProperty = BindableProperty.Create(
        nameof(Branch),
        typeof(int),
        typeof(AdjustableBranchGridLabel),
        -1,
        BindingMode.TwoWay,
        propertyChanged: OnTextChanged);

    public AdjustableBranchGridLabel()
    {
        InitializeComponent();
    }

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (AdjustableBranchGridLabel)bindable;
        var newText = string.Format("{0:D5}", newValue);

        control.BranchLabel.Text = newText;
    }

    private void OnMinusButtonClicked(object sender, EventArgs e)
    {
        if (Branch == 0 || Branch == MaxLimit) return;

        Branch--;
        AdjustCommand.Execute(new ChangeBranchContextModel { Index = Index, Branch = Branch, Changed = "-", Position = Position });
    }

    private void OnPlusButtonClicked(object sender, EventArgs e)
    {
        if (Branch == MaxLimit || (Branch == 0 && Index == 0)) return;

        Branch++;
        AdjustCommand.Execute(new ChangeBranchContextModel { Index = Index, Branch = Branch, Changed = "+", Position = Position });
    }
}