using NAudio.Wave;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InternetRadioStreamer
{

    partial class Form1
    {
        private BufferedWaveProvider bufferedWaveProvider;
        private IWavePlayer waveOut;
        private volatile bool isStreaming;
        private void btnStart_Click(object sender, EventArgs e)
        {
            string url = "http://swr-mp3-s-swr2.akacast.akamaistream.net/7/204/137135/v1/gnl.akacast.akamaistream.net/swr-mp3-s-swr2"; // Replace with your actual stream URL
            Task.Run(() => StreamRadio(url));
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            isStreaming = false;
            waveOut.Stop();
            waveOut.Dispose();
        }

        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.LinkTextBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(288, 38);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "&START";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(399, 38);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 1;
            this.button2.Text = "STO&P";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // LinkTextBox1
            // 
            this.LinkTextBox1.Location = new System.Drawing.Point(12, 12);
            this.LinkTextBox1.Name = "LinkTextBox1";
            this.LinkTextBox1.Size = new System.Drawing.Size(737, 20);
            this.LinkTextBox1.TabIndex = 2;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(761, 163);
            this.Controls.Add(this.LinkTextBox1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.TextBox LinkTextBox1;

        private void StreamRadio(string url)
        {
            try
            {
                isStreaming = true;
                var webRequest = (HttpWebRequest)WebRequest.Create(url);
                using (var response = webRequest.GetResponse())
                using (var responseStream = response.GetResponseStream())
                {
                    var waveStream = new Mp3FileReader(responseStream);
                    bufferedWaveProvider = new BufferedWaveProvider(waveStream.WaveFormat);
                    bufferedWaveProvider.BufferDuration = TimeSpan.FromSeconds(20); // Buffer for 20 seconds

                    waveOut = new WaveOut();
                    waveOut.Init(bufferedWaveProvider);
                    waveOut.Play();

                    var buffer = new byte[16384 * 4]; // 64 KB buffer
                    int bytesRead;
                    while (isStreaming && (bytesRead = waveStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        bufferedWaveProvider.AddSamples(buffer, 0, bytesRead);
                    }
                }
            }
            catch (WebException ex)
            {
                MessageBox.Show($"Error accessing the stream: {ex.Message}", "Streaming Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (waveOut != null)
                {
                    waveOut.Stop();
                    waveOut.Dispose();
                }
            }
        }


    }
    
}

