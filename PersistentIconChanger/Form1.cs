using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace PersistentIconChanger
{
    public partial class Form1 : Form
    {
        private Image selectedImage;
        private string selectedFolder;
        private string selectedImageName;
        private string selectedImagePath;
        private string iniPath;

        public Form1()
        {
            InitializeComponent();
            //Setting openFileDialogFilters
            selectImageDialog.Filter = "Icon Files (*.ico)| *.ico";
            //Inizialization of icon preview picture boxes
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox2.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox3.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox4.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox5.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox6.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox7.SizeMode = PictureBoxSizeMode.StretchImage;
            updateImageBoxes(new Bitmap(Properties.Resources.folder));
        }

        private void selectFolderButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                selectedFolderBox.Text = folderBrowserDialog.SelectedPath;
                selectedFolder = folderBrowserDialog.SelectedPath;
                if (selectedImageBox.Text != "")    //Activate change icon button if image is already selected
                    changeButton.Enabled = true;
                resetButton.Enabled = true;         //Activate reset button as soon as a folder is selected
                iniPath = selectedFolder + "\\desktop.ini";
            }
        }

        private void selectImageButton_Click(object sender, EventArgs e)
        {
            Stream fileStream = null;
            if (selectImageDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if ((fileStream = selectImageDialog.OpenFile()) != null)
                        using (fileStream)
                            selectedImage = Image.FromFile(selectImageDialog.FileName);
                    selectedImageName = selectImageDialog.SafeFileName;     //Get file "name.ico"
                    selectedImagePath = selectImageDialog.FileName;
                    updateImageBoxes(selectedImage);
                    selectedImageBox.Text = selectImageDialog.FileName;
                    if (selectedFolderBox.Text != "")                       //Activate change icon button if folder is already selected
                        changeButton.Enabled = true;
                }
                //Exception management
                catch (OutOfMemoryException)
                {
                    MessageBox.Show("Invalid image format", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (FileNotFoundException)
                {
                    MessageBox.Show("The file does not exist", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (ArgumentException)
                {
                    MessageBox.Show("Filename is a Uri", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Could not read file from disk: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void updateImageBoxes(Image image)
        {
            //Set the same image for each picture box
            pictureBox1.Image = image;
            pictureBox2.Image = image;
            pictureBox3.Image = image;
            pictureBox4.Image = image;
            pictureBox5.Image = image;
            pictureBox6.Image = image;
            pictureBox7.Image = image;
        }

        private void changeButton_Click(object sender, EventArgs e)
        {
            try
            {
                //Erase old icons
                eraseOldIcons();

                //Create an hidden copy of selected icon file
                string copyPath = selectedFolder + "\\" + selectedImageName;
                //If the same icon already exists, set its attributes to normal in order to be able to overwrite it
                if (File.Exists(copyPath))
                    File.SetAttributes(copyPath, FileAttributes.Normal);
                //Copy the icon, ovewriting if necessary
                File.Copy(selectedImagePath, copyPath, true);
                //Set icon file attributes back to hidden
                File.SetAttributes(copyPath, FileAttributes.Hidden);

                //Set the folder attributes to System (necessary for telling windows to refresh icon.db cache)
                File.SetAttributes(selectedFolder, FileAttributes.System);

                //.ini file creation
                string text = getIniContent();
                string iniPath = selectedFolder + "\\desktop.ini";
                //If desktop.ini already exists, set its attributes to normal in order to be able to overwrite it
                if (File.Exists(iniPath))
                {
                    File.SetAttributes(iniPath, FileAttributes.Normal);
                    File.Delete(iniPath);   //Delete old desktop.ini to avoid conflict
                }
                File.WriteAllText(iniPath, text);
                //Set desktop.ini attributes back to hidden, system and to read-only for security
                File.SetAttributes(iniPath, FileAttributes.Hidden | FileAttributes.ReadOnly | FileAttributes.System);

                MessageBox.Show("Icon successfully changed!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error changing the icon: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void resetButton_Click(object sender, EventArgs e)
        {
            try
            {
                //Look for an hidden file in the same path where desktop.ini is saved (File.Exists() does not return true for hidden files)
                if ((new FileInfo(iniPath).Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
                {
                    File.SetAttributes(iniPath, FileAttributes.Normal); //Set desktop.ini attributes to normal to allow deletion
                    File.Delete(iniPath);   //Delete desktop.ini to restore the default folder icon
                    eraseOldIcons();
                    MessageBox.Show("Icon successfully restored!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch(FileNotFoundException)    //If destop.ini file is not found, there is no custom icon set
            {
                MessageBox.Show("The folder icon is already the default one", "Attention", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error restoring default icon: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string getIniContent()
        {
            /* Returns the content necessary for windows to display the icon, with the following structure:
             * [.ShellClassInfo]
             * IconResource="fileName".ico, 0
             * [ViewState]
             * Mode =
             * Vid =
             * FolderType = Generic
             */
             return "[.ShellClassInfo]\nIconResource=" + selectedImageName + ",0\n[ViewState]\nMode =\nVid =\nFolderType = Generic";
        }

        private void eraseOldIcons()
        {
            //Delete all hidden .ico files in the selected folder
            string[] files = Directory.GetFiles(selectedFolder);
            foreach(string s in files)
            {
                if ((new FileInfo(s).Attributes & FileAttributes.Hidden) == FileAttributes.Hidden && Path.GetExtension(s).Equals(".ico"))
                {
                    File.SetAttributes(s, FileAttributes.Normal);
                    File.Delete(s);
                    Console.WriteLine(s + " deleted.");
                }
            }
        }
    }
}