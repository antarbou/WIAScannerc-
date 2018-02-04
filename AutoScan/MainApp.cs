using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using WIA;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;

namespace AutoScan
{
    public partial class MainApp : Form
    {
        public MainApp()
        {
            InitializeComponent();
        }


        int newDoc = 0;
        int cmbCMIndex = 0;
        Stopwatch sw = new Stopwatch();

        private void btnStartScan_Click(object sender, EventArgs e)
        {
            bgwScan.RunWorkerAsync(5000);
            btnStartScan.Enabled = false;
            btnStopScan.Enabled = true;
            cmbColorMode.Enabled = false;
            nudWidthInch.Enabled = false;
            nudHeightInch.Enabled = false;
            nudRes.Enabled = false;
            cbxCustomPixel.Enabled = false;
            nudHeight.Enabled = false;
            nudWidth.Enabled = false;

            cmbCMIndex = cmbColorMode.SelectedIndex;
        }



        //private void btnSelectPath_Click(object sender, EventArgs e)
        //{
        //    FolderBrowserDialog folderDlg = new FolderBrowserDialog();
        //    folderDlg.ShowNewFolderButton = true;
        //    DialogResult result = folderDlg.ShowDialog();
        //    if (result == DialogResult.OK)
        //    {
        //        txtPath.Text = folderDlg.SelectedPath;
        //    }
        //}

        private void frmAutoScan_Load(object sender, EventArgs e)
        {
            //txtPath.Text = Path.GetTempPath();
            string foldername = Path.GetFullPath(@"\image\")+DateTime.Now.ToString("yyyyMMdd");
            //Directory.CreateDirectory(@"\ashok");

            Directory.CreateDirectory(foldername);
            //txtPath.Text = Path.GetFullPath(@"\image");
            String txtPath = Path.GetFullPath(foldername);
            nudHeightInch.Value = 11;
            nudWidthInch.Value = 8;
            cmbColorMode.SelectedIndex = 1;
        }


        private void bgwScan_DoWork(object sender, DoWorkEventArgs e)
        {
            while (!bgwScan.CancellationPending)
            {
                if (newDoc == 0)
                {
                    newDoc = 1;
                    ScanDoc();

                }
            }

        }

        private void btnStopScan_Click(object sender, EventArgs e)
        {
            bgwScan.CancelAsync();
            btnStartScan.Enabled = true;
            btnStopScan.Enabled = false;
            cmbColorMode.Enabled = true;

            //
            btnStartScan.Enabled = true;
            btnStopScan.Enabled = false;
            cmbColorMode.Enabled = true;
            nudWidthInch.Enabled = true;
            nudHeightInch.Enabled = true;
            nudRes.Enabled = true;
            cbxCustomPixel.Enabled = true;
            if (cbxCustomPixel.Checked == true)
            {
                nudHeight.Enabled = true;
                nudWidth.Enabled = true;
            }
            sw.Reset();
            sw.Stop();

        }

        private static void AdjustScannerSettings(IItem scannnerItem, int scanResolutionDPI, int scanStartLeftPixel, int scanStartTopPixel,
                    int scanWidthPixels, int scanHeightPixels, int brightnessPercents, int contrastPercents, int colorMode)
        {
            const string WIA_SCAN_COLOR_MODE = "6146";
            const string WIA_HORIZONTAL_SCAN_RESOLUTION_DPI = "6147";
            const string WIA_VERTICAL_SCAN_RESOLUTION_DPI = "6148";
            const string WIA_HORIZONTAL_SCAN_START_PIXEL = "6149";
            const string WIA_VERTICAL_SCAN_START_PIXEL = "6150";
            const string WIA_HORIZONTAL_SCAN_SIZE_PIXELS = "6151";
            const string WIA_VERTICAL_SCAN_SIZE_PIXELS = "6152";
            const string WIA_SCAN_BRIGHTNESS_PERCENTS = "6154";
            const string WIA_SCAN_CONTRAST_PERCENTS = "6155";
            SetWIAProperty(scannnerItem.Properties, WIA_HORIZONTAL_SCAN_RESOLUTION_DPI, scanResolutionDPI);
            SetWIAProperty(scannnerItem.Properties, WIA_VERTICAL_SCAN_RESOLUTION_DPI, scanResolutionDPI);
            SetWIAProperty(scannnerItem.Properties, WIA_HORIZONTAL_SCAN_START_PIXEL, scanStartLeftPixel);
            SetWIAProperty(scannnerItem.Properties, WIA_VERTICAL_SCAN_START_PIXEL, scanStartTopPixel);
            SetWIAProperty(scannnerItem.Properties, WIA_HORIZONTAL_SCAN_SIZE_PIXELS, scanWidthPixels);
            SetWIAProperty(scannnerItem.Properties, WIA_VERTICAL_SCAN_SIZE_PIXELS, scanHeightPixels);
            SetWIAProperty(scannnerItem.Properties, WIA_SCAN_BRIGHTNESS_PERCENTS, brightnessPercents);
            SetWIAProperty(scannnerItem.Properties, WIA_SCAN_CONTRAST_PERCENTS, contrastPercents);
            SetWIAProperty(scannnerItem.Properties, WIA_SCAN_COLOR_MODE, colorMode);



        }

        private static void SetWIAProperty(IProperties properties, object propName, object propValue)
        {
            Property prop = properties.get_Item(ref propName);
            prop.set_Value(ref propValue);
        }

        private static void SaveImageToPNG(ImageFile image, string fileName)
        {
            ImageProcess imgProcess = new ImageProcess();
            object convertFilter = "Convert";
            string convertFilterID = imgProcess.FilterInfos.get_Item(ref convertFilter).FilterID;
            imgProcess.Filters.Add(convertFilterID, 0);
            SetWIAProperty(imgProcess.Filters[imgProcess.Filters.Count].Properties, "FormatID", WIA.FormatID.wiaFormatPNG);
            image = imgProcess.Apply(image);
            image.SaveFile(fileName);
        }
        private void ScanDoc()
        {
            try
            {
                CommonDialogClass commonDialogClass = new CommonDialogClass();
                Device scannerDevice = commonDialogClass.ShowSelectDevice(WiaDeviceType.ScannerDeviceType, false, false);
                if (scannerDevice != null)
                {
                    Item scannnerItem = scannerDevice.Items[1];
                    AdjustScannerSettings(scannnerItem, (int)nudRes.Value, 0, 0, (int)nudWidth.Value, (int)nudHeight.Value, 0, 0, cmbCMIndex);

                    object scanResult = commonDialogClass.ShowTransfer(scannnerItem, WIA.FormatID.wiaFormatPNG, false);
                    //picScan.Image = (System.Drawing.Image)scanResult;
                    if (scanResult != null)
                    {
                        ImageFile image = (ImageFile)scanResult;
                        string fileName = "";

                        string foldername = Path.GetFullPath(@"\image\") + DateTime.Now.ToString("yyyyMMdd");
                        string txtPath = Path.GetFullPath(foldername);
                        var files = Directory.GetFiles(txtPath, "*.PNG");

                        try
                        {
                            string f = ((files.Max(p1 => Int32.Parse(Path.GetFileNameWithoutExtension(p1)))) + 1).ToString();
                            fileName = txtPath + "\\" + f + ".PNG";
                        }
                        catch (Exception ex)
                        {
                            fileName = txtPath + "\\" + "1.PNG";
                        }
                        SaveImageToPNG(image, fileName);
                        picScan.ImageLocation = fileName;
                    }
                }
            }
            catch(Exception err)
            {
                MessageBox.Show("Check the Device Connection \n or \n Change the Scanner Device", "Devic Not Found!", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            
        }

        private void nudWidthInch_ValueChanged(object sender, EventArgs e)
        {
            nudWidth.Value = nudRes.Value * nudWidthInch.Value;
        }

        private void nudHeightInch_ValueChanged(object sender, EventArgs e)
        {
            nudHeight.Value = nudRes.Value * nudHeightInch.Value;
        }

        private void nudRes_ValueChanged(object sender, EventArgs e)
        {
            nudWidth.Value = nudRes.Value * nudWidthInch.Value;

            nudHeight.Value = nudRes.Value * nudHeightInch.Value;
        }

        private void cbxCustomPixel_CheckedChanged(object sender, EventArgs e)
        {
            if (cbxCustomPixel.Checked == true)
            {
                nudWidth.ReadOnly = false;
                nudHeight.ReadOnly = false;
            }
            else
            {
                nudWidth.ReadOnly = true;
                nudHeight.ReadOnly = true;
            }
        }
    }
}
