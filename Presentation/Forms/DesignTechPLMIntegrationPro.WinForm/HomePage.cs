using DesignTechPLMIntegrationPro.Application.Interfaces.CAD.PDF;
using DesignTechPLMIntegrationPro.Application.Interfaces.Log;
using DesignTechPLMIntegrationPro.Application.Interfaces.WTPart.Alternate;
using DesignTechPLMIntegrationPro.Application.Interfaces.WTPart.State;
using DesignTechPLMIntegrationPro.WinForm.Pages;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesignTechPLMIntegrationPro.WinForm
{
    public partial class HomePage : Form
    {
        private readonly ILogService _logService;
        private readonly IStateService _stateService;
        private readonly IPdfService _pdfService;
        private readonly IAlternateStateService _alternateStateService;


        public HomePage(ILogService logService, IStateService stateService, IPdfService pdfService, IAlternateStateService alternateStateService)
        {
            InitializeComponent();
            _logService = logService;
            _stateService = stateService;
            _pdfService = pdfService;

            _logService.OnLog += LogService_OnLog;
            _alternateStateService = alternateStateService;
        }

        private void LogService_OnLog(string message)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new Action(() => listBox1.Items.Add(message)));
            }
            else
            {
                listBox1.Items.Add(message);
            }
        }

        private void HomePage_Load(object sender, EventArgs e)
        {

        }

        public async Task RunFunc()
        {
            try
            {

                #region Task dönüyor ise bu

                await Task.WhenAll(
              _stateService.RELEASED(),
              _stateService.INWORK(),
              _stateService.CANCELLED()
              );
                #endregion

                #region Task harici başka birşey dönüyor ise (string, Int, Void vs.) bu

                //Thread threadReleased = new Thread(() => _stateService.RELEASED("1"));
                //Thread threadInwork = new Thread(() => _stateService.INWORK("1"));
                //Thread threadCancelled = new Thread(() => _stateService.CANCELLED("1"));
                //threadReleased.Start();
                //threadInwork.Start();
                //threadCancelled.Start();
                #endregion
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private async void btnStart_Click(object sender, EventArgs e)
        {
            try
            {



                await Task.WhenAll(
           //_stateService.RELEASED(),
           //_stateService.INWORK(),
           //_stateService.CANCELLED(),
           _pdfService.SendToCadPDF()
           //_alternateStateService.AlternateState(),
           );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private async void başlatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {



                await Task.WhenAll(
           //_stateService.RELEASED(),
           //_stateService.INWORK(),
           //_stateService.CANCELLED(),
           _pdfService.SendToCadPDF()
           //_alternateStateService.AlternateState(),
           );
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void sQLBağlantıAyarlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlAyarlari sqlAyarlari = new SqlAyarlari();
            sqlAyarlari.ShowDialog(this);
        }

        private void windchillBağlantıAyarlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindchillAyarlari windchillAyarlari = new WindchillAyarlari();
            windchillAyarlari.ShowDialog(this);
        }

        private void lOGAyarlarıToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }
    }
}
