using Microsoft.Win32;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpriteGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Collections.ObjectModel.ObservableCollection<string> _images = new System.Collections.ObjectModel.ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();
            SpriteListBox.ItemsSource = _images;
        }

        private void BrowseOutput_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();

            dialog.Title = "Select Output File";
            dialog.Filter = "PNG Image (*.png)|*.png";
            dialog.DefaultExt = ".png";
            dialog.AddExtension = true;
            dialog.FileName = "SpriteSheet.png";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                // Full path
                string fullPath = dialog.FileName;

                // Split into directory and filename
                OutputFileTextBox.Text = System.IO.Path.GetDirectoryName(fullPath);
                FilenameTextBox.Text = System.IO.Path.GetFileName(fullPath);
            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp|All Files|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string filename in openFileDialog.FileNames)
                {
                    if (!_images.Contains(filename))
                    {
                        _images.Add(filename);
                    }
                }
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SpriteListBox.SelectedItem != null)
            {
                _images.Remove((string)SpriteListBox.SelectedItem);
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(OutputFileTextBox.Text) || string.IsNullOrWhiteSpace(FilenameTextBox.Text))
            {
                MessageBox.Show("Please select an output directory and filename.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!int.TryParse(ColumnsTextBox.Text, out int columns) || columns < 1)
            {
                MessageBox.Show("Please enter a valid number of columns (>= 1).", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (_images.Count == 0)
            {
                MessageBox.Show("Please add at least one image.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                TextureAtlasLib.Spritesheet spritesheet = new TextureAtlasLib.Spritesheet();
                spritesheet.InputPaths = new System.Collections.Generic.List<string>(_images);
                spritesheet.OutputDirectory = OutputFileTextBox.Text;
                spritesheet.OutputFile = FilenameTextBox.Text;
                spritesheet.Columns = columns;
                spritesheet.IncludeMetaData = MetaDataCheckBox.IsChecked == true;

                spritesheet.Generate(true);

                var result = MessageBox.Show("Generated successfully. Would you like to view it?", "Success", MessageBoxButton.YesNo, MessageBoxImage.Information);

                if (result == MessageBoxResult.Yes)
                {
                    System.Diagnostics.Process.Start("explorer.exe", OutputFileTextBox.Text);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating sprite sheet: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string _currentProjectPath = null;
        private bool _hasUnsavedChanges = false;

        // ... Existing methods ...

        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (CheckUnsavedChanges())
            {
                ClearUI();
                _currentProjectPath = null;
                ProjectNameTextBlock.Text = "New Project";
                SaveMenuItem.IsEnabled = false;
                _hasUnsavedChanges = false;
            }
        }

        private void Open_Click(object sender, RoutedEventArgs e)
        {
            if (CheckUnsavedChanges())
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";

                if (openFileDialog.ShowDialog() == true)
                {
                    LoadProject(openFileDialog.FileName);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_currentProjectPath))
            {
                SaveProject(_currentProjectPath);
            }
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";

            if (saveFileDialog.ShowDialog() == true)
            {
                SaveProject(saveFileDialog.FileName);
            }
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (CheckUnsavedChanges())
            {
                Application.Current.Shutdown();
            }
        }

        private bool CheckUnsavedChanges()
        {
            // Simplified check: always return true unless implemented robust change tracking or data exists
            // For this assignment, the prompt requirements are specific.
            // Let's implement a simpler version based on user request: "prompts... if user would like to save first"

            if (_images.Count > 0 || !string.IsNullOrEmpty(OutputFileTextBox.Text))
            {
                MessageBoxResult result = MessageBox.Show("Do you want to save changes to the current project?", "Sprite Generator", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (!string.IsNullOrEmpty(_currentProjectPath))
                    {
                        SaveProject(_currentProjectPath);
                        return true;
                    }
                    else
                    {
                        SaveFileDialog saveFileDialog = new SaveFileDialog();
                        saveFileDialog.Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*";
                        if (saveFileDialog.ShowDialog() == true)
                        {
                            SaveProject(saveFileDialog.FileName);
                            return true;
                        }
                        return false;
                    }
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    return false;
                }
            }
            return true;
        }

        private void SaveProject(string path)
        {
            try
            {
                TextureAtlasLib.Spritesheet spritesheet = new TextureAtlasLib.Spritesheet();
                spritesheet.InputPaths = new System.Collections.Generic.List<string>(_images);
                spritesheet.OutputDirectory = OutputFileTextBox.Text;
                spritesheet.OutputFile = FilenameTextBox.Text;
                int.TryParse(ColumnsTextBox.Text, out int columns);
                spritesheet.Columns = columns;
                spritesheet.IncludeMetaData = MetaDataCheckBox.IsChecked == true;

                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(TextureAtlasLib.Spritesheet));
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(path))
                {
                    serializer.Serialize(writer, spritesheet);
                }

                _currentProjectPath = path;
                ProjectNameTextBlock.Text = System.IO.Path.GetFileName(path);
                SaveMenuItem.IsEnabled = true;
                _hasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving project: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProject(string path)
        {
            try
            {
                System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(TextureAtlasLib.Spritesheet));
                using (System.IO.StreamReader reader = new System.IO.StreamReader(path))
                {
                    TextureAtlasLib.Spritesheet spritesheet = (TextureAtlasLib.Spritesheet)serializer.Deserialize(reader);

                    ClearUI();

                    if (spritesheet.InputPaths != null)
                    {
                        foreach (var img in spritesheet.InputPaths)
                        {
                            _images.Add(img);
                        }
                    }

                    OutputFileTextBox.Text = spritesheet.OutputDirectory;
                    FilenameTextBox.Text = spritesheet.OutputFile;
                    ColumnsTextBox.Text = spritesheet.Columns.ToString();
                    MetaDataCheckBox.IsChecked = spritesheet.IncludeMetaData;
                }

                _currentProjectPath = path;
                ProjectNameTextBlock.Text = System.IO.Path.GetFileName(path);
                SaveMenuItem.IsEnabled = true;
                _hasUnsavedChanges = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading project: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearUI()
        {
            _images.Clear();
            OutputFileTextBox.Text = "";
            FilenameTextBox.Text = "";
            ColumnsTextBox.Text = "";
            MetaDataCheckBox.IsChecked = false;
        }
    }
}