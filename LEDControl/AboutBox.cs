using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace LEDControl
{
    // About dialog with clickable links (a plain MessageBox cannot make URLs clickable).
    public class AboutBox : Form
    {
        const int ContentWidth = 430;

        public AboutBox(string driver, bool isAdmin)
        {
            Text = "About ThinkPad Toolbox";
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowInTaskbar = false;
            ClientSize = new Size(470, 396);
            try { Icon = LEDControl.Properties.Resources.AppIcon; } catch { }

            var panel = new FlowLayoutPanel
            {
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Dock = DockStyle.Fill,
                Padding = new Padding(16, 14, 16, 8)
            };

            panel.Controls.Add(TitleLabel("ThinkPad Toolbox 1.0.0"));
            panel.Controls.Add(Para("Copyright © 2026 Aung Ko Ko  ·  MIT License"));
            panel.Controls.Add(Gap(6));
            panel.Controls.Add(LinkRow("Project page", "https://github.com/aungkokomm/ThinkPad-Toolbox"));
            panel.Controls.Add(LinkRow("Author", "https://github.com/aungkokomm"));
            panel.Controls.Add(Gap(8));
            panel.Controls.Add(Para("This application is NOT affiliated with, endorsed by, or sponsored by Lenovo. \"Lenovo\" and \"ThinkPad\" are trademarks of Lenovo."));
            panel.Controls.Add(Gap(6));
            panel.Controls.Add(Para("Driver: " + driver + "      Running as administrator: " + (isAdmin ? "Yes" : "No")));
            panel.Controls.Add(Gap(8));
            panel.Controls.Add(Para("Based on ValiNet's ThinkPad LED Control (ISC License) and TPFanControl. The bundled WinRing0 driver is © OpenLibSys.org. PawnIO is a separate, independently licensed driver."));
            panel.Controls.Add(LinkRow("ThinkPad LED Control (ValiNet)", "https://github.com/valinet/ThinkPadLEDControl"));
            panel.Controls.Add(LinkRow("PawnIO", "https://pawnio.eu"));

            var buttonBar = new Panel { Dock = DockStyle.Bottom, Height = 46 };
            var ok = new Button { Text = "OK", DialogResult = DialogResult.OK, Size = new Size(90, 28) };
            ok.Location = new Point(ClientSize.Width - ok.Width - 16, 9);
            ok.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            buttonBar.Controls.Add(ok);

            Controls.Add(panel);
            Controls.Add(buttonBar);
            AcceptButton = ok;
            CancelButton = ok;
        }

        static Label TitleLabel(string s)
        {
            return new Label
            {
                Text = s,
                AutoSize = true,
                Font = new Font(SystemFonts.MessageBoxFont, FontStyle.Bold),
                MaximumSize = new Size(ContentWidth, 0),
                Margin = new Padding(0)
            };
        }

        static Label Para(string s)
        {
            return new Label
            {
                Text = s,
                AutoSize = true,
                MaximumSize = new Size(ContentWidth, 0),
                Margin = new Padding(0)
            };
        }

        static Control Gap(int h)
        {
            return new Label { Text = "", AutoSize = false, Size = new Size(1, h), Margin = new Padding(0) };
        }

        static LinkLabel LinkRow(string caption, string url)
        {
            var l = new LinkLabel
            {
                Text = caption + ":  " + url,
                AutoSize = true,
                MaximumSize = new Size(ContentWidth, 0),
                Margin = new Padding(0, 1, 0, 1)
            };
            // Make only the URL portion the clickable link.
            l.LinkArea = new LinkArea(caption.Length + 3, url.Length);
            l.LinkClicked += (s, e) =>
            {
                try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
                catch { }
            };
            return l;
        }
    }
}
