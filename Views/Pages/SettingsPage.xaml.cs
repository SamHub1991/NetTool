using System.Windows;
using System.Windows.Controls;

namespace DeerFlow.WPF.Views.Pages;

public partial class SettingsPage : UserControl
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    private void ToggleExperimentCenter_Click(object sender, RoutedEventArgs e)
    {
        if (DocumentGenerationPanel is not null)
        {
            DocumentGenerationPanel.Visibility = Visibility.Collapsed;
        }

        if (ExperimentCenterPanel is not null)
        {
            ExperimentCenterPanel.Visibility = 
                ExperimentCenterPanel.Visibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
        }
    }

    private void ToggleDocumentGeneration_Click(object sender, RoutedEventArgs e)
    {
        if (ExperimentCenterPanel is not null)
        {
            ExperimentCenterPanel.Visibility = Visibility.Collapsed;
        }

        if (DocumentGenerationPanel is not null)
        {
            DocumentGenerationPanel.Visibility = 
                DocumentGenerationPanel.Visibility == Visibility.Visible
                    ? Visibility.Collapsed
                    : Visibility.Visible;
        }
    }
}
