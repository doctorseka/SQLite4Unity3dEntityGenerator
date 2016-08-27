using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SqliteEntityGenerator.Classes;
using System.IO;

namespace SqliteEntityGenerator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DatabaseAnalyzer analyzer = null;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".db";
            dlg.Filter = "SqLite DB Files (*.db)|*.db|All Files (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method 
            bool? result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                txtDbPath.Text = filename;

                analyzer = new DatabaseAnalyzer(filename);
                if(analyzer.Load())
                    FillList();
            }
            
        }

        void FillList()
        {
            lstClasses.Items.Clear();
            btnSave.IsEnabled = false;
            if (analyzer == null)
                return;

            List<string> tables = analyzer.GetAllTables();

            foreach(string tbl in tables)
            {
                lstClasses.Items.Add(new System.Windows.Controls.CheckBox() {
                    Content = tbl
                });
            }

            btnSave.IsEnabled = true;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();

            if (result != System.Windows.Forms.DialogResult.OK)
                return;

            List<string> selectedTables = lstClasses.Items.OfType<System.Windows.Controls.CheckBox>().Where(el => el.IsChecked == true).Select(el => el.Content as string).ToList();

            foreach (string tableName in selectedTables)
            {
                string code = analyzer.BuildClassCode(tableName);

                string filename = tableName + ".cs";

                using (StreamWriter writer = File.CreateText(Path.Combine(dialog.SelectedPath, filename)))
                {
                    writer.Write(code);
                    writer.Close();
                }
            }



        }
    }
}
