using iTextSharp.text.pdf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PDFCounter
{
    public partial class PDFCounter : Form
    {
        public string SelectedPath { get; set; }
        public List<string> FilesFound { get; set; } = new List<string>();
        public int ErrorsCount { get; set; }
        public delegate void ShowInProgressTask();
        public delegate void ShowFilesFound();
        public delegate void ShowCurrentStatusPages(string count);
        public delegate void ShowCurrentStatusFiles(string count);
        delegate void MsgShowErrors(string message);
        public PDFCounter()
        {
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            pdfFolderdialog.RootFolder = Environment.SpecialFolder.MyComputer;
            pdfFolderdialog.ShowDialog();
            SelectedPath = pdfFolderdialog.SelectedPath;
            txtPDFFolder.Text = SelectedPath;
            btnStart.Enabled = false;
            GesFilesFromSystem.RunWorkerAsync();

        }

        private void GesFilesFromSystem_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] argsObjectError = new object[1];
            BeginInvoke(new ShowInProgressTask(this.ShowInProgress));
            try
            {
                FilesFound = Directory.GetFiles(SelectedPath, "*.pdf*", SearchOption.AllDirectories).ToList();
            }
            catch (Exception ex)
            {
                argsObjectError[0] = ex.Message;
                BeginInvoke(new MsgShowErrors(this.ShowMsgError), argsObjectError);
                
            }
        }

        private void GesFilesFromSystem_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BeginInvoke(new ShowFilesFound(this.ShowCompletedSearch));

        }
        void ShowInProgress()
        {
            lblIndicator.Visible = true;
            lblIndicator.Text = "Working...";
        }
        void ShowCompletedSearch()
        {
            lblIndicator.ForeColor = Color.DarkBlue;
            lblIndicator.Text = $"Files found: {FilesFound.Count}";
            btnStart.Enabled = true;
            
        }
        void ShowCountingPagesIndicator()
        {
            lblIndicator.Text = "Counting Pages and Files";
        }
        void ShowCurrentPagesStr(string count)
        {
            lblResultPages.Text = count;

        }
        void ShowCurrentFilesStr(string count)
        {
            lblResultFiles.Text = count;

        }
        void ShowCurrentErrors(string count)
        {
            lblErrors.Text = count;
        }
        void ShowCompletedProcess()
        {
            lblIndicator.Text = "Process Completed";
            btnStart.Enabled = true;
            btnBrowse.Enabled = true;
        }
        void ShowMsgError(string message)
        {
            MessageBox.Show(message, "Error",MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void CountPDFPages_DoWork(object sender, DoWorkEventArgs e)
        {
            object[] argsObjectErrors = new object[1];
            try
            {
                object[] argsObjectPages = new object[1];
                object[] argsObjectFiles = new object[1];
                int fileCounter = 0;
                int pageCounter = 0;
                foreach (var filePath in FilesFound)
                {
                    try
                    {
                        using (PdfReader pdfReader = new PdfReader(filePath))
                        {
                            pageCounter += pdfReader.NumberOfPages;
                        }
                        argsObjectPages[0] = pageCounter.ToString();
                        BeginInvoke(new ShowCurrentStatusPages(this.ShowCurrentPagesStr), argsObjectPages);

                        fileCounter++;
                        argsObjectFiles[0] = fileCounter.ToString();
                        BeginInvoke(new ShowCurrentStatusFiles(this.ShowCurrentFilesStr), argsObjectFiles);
                    }
                    catch (iTextSharp.text.exceptions.BadPasswordException)
                    {
                        ErrorsCount++;    
                    }
                    catch(iTextSharp.text.exceptions.InvalidPdfException)
                    {
                        ErrorsCount++;
                    }
                    argsObjectErrors[0] = ErrorsCount.ToString();
                    BeginInvoke(new ShowCurrentStatusFiles(this.ShowCurrentErrors), argsObjectErrors);

                }
            }
            catch (AggregateException aex)
            {
                throw aex;
            }
            
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            if (FilesFound.Count > 0)
            {
                try
                {
                    CountPDFPages.RunWorkerAsync();
                    ShowCountingPagesIndicator();
                    btnStart.Enabled = false;
                    btnBrowse.Enabled = false;
                }
                catch (Exception ex)
                {

                    ShowMsgError(ex.Message);
                }
                
            }
            
        }

        private void CountPDFPages_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BeginInvoke(new ShowFilesFound(this.ShowCompletedProcess));
        }
    }
}
