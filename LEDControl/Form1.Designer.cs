namespace LEDControl
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            // LEDs
            this.gbLeds = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.powerOn = new System.Windows.Forms.Button();
            this.powerOff = new System.Windows.Forms.Button();
            this.powerBlink = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.dotOn = new System.Windows.Forms.Button();
            this.dotOff = new System.Windows.Forms.Button();
            this.dotBlink = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.microphoneOn = new System.Windows.Forms.Button();
            this.microphoneOff = new System.Windows.Forms.Button();
            this.microphoneBlink = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.sleepOn = new System.Windows.Forms.Button();
            this.sleepOff = new System.Windows.Forms.Button();
            this.sleepBlink = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.button11 = new System.Windows.Forms.Button();
            this.button10 = new System.Windows.Forms.Button();
            this.button9 = new System.Windows.Forms.Button();
            // Fan
            this.gbFan = new System.Windows.Forms.GroupBox();
            this.fanAuto = new System.Windows.Forms.Button();
            this.fanFull = new System.Windows.Forms.Button();
            // Keyboard
            this.gbKeyboard = new System.Windows.Forms.GroupBox();
            this.kbOff = new System.Windows.Forms.Button();
            this.kbLow = new System.Windows.Forms.Button();
            this.kbHigh = new System.Windows.Forms.Button();
            // Status
            this.gbStatus = new System.Windows.Forms.GroupBox();
            this.lblSensors = new System.Windows.Forms.Label();
            this.lblBattery = new System.Windows.Forms.Label();
            // Options
            this.gbOptions = new System.Windows.Forms.GroupBox();
            this.rememberKBD = new System.Windows.Forms.CheckBox();
            this.checkTurnKBLightOff = new System.Windows.Forms.CheckBox();
            // Bottom buttons
            this.button1 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button12 = new System.Windows.Forms.Button();
            // Non-visual
            this.NotifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lightTimer = new System.Windows.Forms.Timer(this.components);
            this.statusTimer = new System.Windows.Forms.Timer(this.components);
            this.contextMenuStrip1.SuspendLayout();
            this.gbLeds.SuspendLayout();
            this.gbFan.SuspendLayout();
            this.gbKeyboard.SuspendLayout();
            this.gbStatus.SuspendLayout();
            this.gbOptions.SuspendLayout();
            this.SuspendLayout();
            //
            // gbLeds
            //
            this.gbLeds.Controls.Add(this.label1);
            this.gbLeds.Controls.Add(this.powerOn);
            this.gbLeds.Controls.Add(this.powerOff);
            this.gbLeds.Controls.Add(this.powerBlink);
            this.gbLeds.Controls.Add(this.label2);
            this.gbLeds.Controls.Add(this.dotOn);
            this.gbLeds.Controls.Add(this.dotOff);
            this.gbLeds.Controls.Add(this.dotBlink);
            this.gbLeds.Controls.Add(this.label3);
            this.gbLeds.Controls.Add(this.microphoneOn);
            this.gbLeds.Controls.Add(this.microphoneOff);
            this.gbLeds.Controls.Add(this.microphoneBlink);
            this.gbLeds.Controls.Add(this.label4);
            this.gbLeds.Controls.Add(this.sleepOn);
            this.gbLeds.Controls.Add(this.sleepOff);
            this.gbLeds.Controls.Add(this.sleepBlink);
            this.gbLeds.Controls.Add(this.label14);
            this.gbLeds.Controls.Add(this.button11);
            this.gbLeds.Controls.Add(this.button10);
            this.gbLeds.Controls.Add(this.button9);
            this.gbLeds.Location = new System.Drawing.Point(12, 6);
            this.gbLeds.Name = "gbLeds";
            this.gbLeds.Size = new System.Drawing.Size(436, 200);
            this.gbLeds.TabIndex = 0;
            this.gbLeds.TabStop = false;
            this.gbLeds.Text = "LEDs";
            //
            // label1
            //
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 26);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 17);
            this.label1.Text = "Power button:";
            //
            // powerOn
            //
            this.powerOn.Location = new System.Drawing.Point(112, 20);
            this.powerOn.Size = new System.Drawing.Size(100, 28);
            this.powerOn.TabIndex = 1;
            this.powerOn.Text = "ON";
            this.powerOn.UseVisualStyleBackColor = true;
            this.powerOn.Click += new System.EventHandler(this.powerOn_Click);
            //
            // powerOff
            //
            this.powerOff.Location = new System.Drawing.Point(218, 20);
            this.powerOff.Size = new System.Drawing.Size(100, 28);
            this.powerOff.TabIndex = 2;
            this.powerOff.Text = "OFF";
            this.powerOff.UseVisualStyleBackColor = true;
            this.powerOff.Click += new System.EventHandler(this.powerOff_Click);
            //
            // powerBlink
            //
            this.powerBlink.Location = new System.Drawing.Point(324, 20);
            this.powerBlink.Size = new System.Drawing.Size(100, 28);
            this.powerBlink.TabIndex = 3;
            this.powerBlink.Text = "Third state";
            this.powerBlink.UseVisualStyleBackColor = true;
            this.powerBlink.Click += new System.EventHandler(this.powerBlink_Click);
            //
            // label2
            //
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 17);
            this.label2.Text = "Red Dot:";
            //
            // dotOn
            //
            this.dotOn.Location = new System.Drawing.Point(112, 56);
            this.dotOn.Size = new System.Drawing.Size(100, 28);
            this.dotOn.TabIndex = 4;
            this.dotOn.Text = "ON";
            this.dotOn.UseVisualStyleBackColor = true;
            this.dotOn.Click += new System.EventHandler(this.dotOn_Click);
            //
            // dotOff
            //
            this.dotOff.Location = new System.Drawing.Point(218, 56);
            this.dotOff.Size = new System.Drawing.Size(100, 28);
            this.dotOff.TabIndex = 5;
            this.dotOff.Text = "OFF";
            this.dotOff.UseVisualStyleBackColor = true;
            this.dotOff.Click += new System.EventHandler(this.dotOff_Click);
            //
            // dotBlink
            //
            this.dotBlink.Location = new System.Drawing.Point(324, 56);
            this.dotBlink.Size = new System.Drawing.Size(100, 28);
            this.dotBlink.TabIndex = 6;
            this.dotBlink.Text = "Third state";
            this.dotBlink.UseVisualStyleBackColor = true;
            this.dotBlink.Click += new System.EventHandler(this.dotBlink_Click);
            //
            // label3
            //
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 98);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(86, 17);
            this.label3.Text = "Microphone:";
            //
            // microphoneOn
            //
            this.microphoneOn.Location = new System.Drawing.Point(112, 92);
            this.microphoneOn.Size = new System.Drawing.Size(100, 28);
            this.microphoneOn.TabIndex = 7;
            this.microphoneOn.Text = "ON";
            this.microphoneOn.UseVisualStyleBackColor = true;
            this.microphoneOn.Click += new System.EventHandler(this.microphoneOn_Click);
            //
            // microphoneOff
            //
            this.microphoneOff.Location = new System.Drawing.Point(218, 92);
            this.microphoneOff.Size = new System.Drawing.Size(100, 28);
            this.microphoneOff.TabIndex = 8;
            this.microphoneOff.Text = "OFF";
            this.microphoneOff.UseVisualStyleBackColor = true;
            this.microphoneOff.Click += new System.EventHandler(this.microphoneOff_Click);
            //
            // microphoneBlink
            //
            this.microphoneBlink.Location = new System.Drawing.Point(324, 92);
            this.microphoneBlink.Size = new System.Drawing.Size(100, 28);
            this.microphoneBlink.TabIndex = 9;
            this.microphoneBlink.Text = "Third state";
            this.microphoneBlink.UseVisualStyleBackColor = true;
            this.microphoneBlink.Click += new System.EventHandler(this.microphoneBlink_Click);
            //
            // label4
            //
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 134);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 17);
            this.label4.Text = "Sleep moon:";
            //
            // sleepOn
            //
            this.sleepOn.Location = new System.Drawing.Point(112, 128);
            this.sleepOn.Size = new System.Drawing.Size(100, 28);
            this.sleepOn.TabIndex = 10;
            this.sleepOn.Text = "ON";
            this.sleepOn.UseVisualStyleBackColor = true;
            this.sleepOn.Click += new System.EventHandler(this.sleepOn_Click);
            //
            // sleepOff
            //
            this.sleepOff.Location = new System.Drawing.Point(218, 128);
            this.sleepOff.Size = new System.Drawing.Size(100, 28);
            this.sleepOff.TabIndex = 11;
            this.sleepOff.Text = "OFF";
            this.sleepOff.UseVisualStyleBackColor = true;
            this.sleepOff.Click += new System.EventHandler(this.sleepOff_Click);
            //
            // sleepBlink
            //
            this.sleepBlink.Location = new System.Drawing.Point(324, 128);
            this.sleepBlink.Size = new System.Drawing.Size(100, 28);
            this.sleepBlink.TabIndex = 12;
            this.sleepBlink.Text = "Third state";
            this.sleepBlink.UseVisualStyleBackColor = true;
            this.sleepBlink.Click += new System.EventHandler(this.sleepBlink_Click);
            //
            // label14
            //
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(10, 170);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(62, 17);
            this.label14.Text = "Fn Lock:";
            //
            // button11
            //
            this.button11.Location = new System.Drawing.Point(112, 164);
            this.button11.Size = new System.Drawing.Size(100, 28);
            this.button11.TabIndex = 13;
            this.button11.Text = "ON";
            this.button11.UseVisualStyleBackColor = true;
            this.button11.Click += new System.EventHandler(this.button11_Click);
            //
            // button10
            //
            this.button10.Location = new System.Drawing.Point(218, 164);
            this.button10.Size = new System.Drawing.Size(100, 28);
            this.button10.TabIndex = 14;
            this.button10.Text = "OFF";
            this.button10.UseVisualStyleBackColor = true;
            this.button10.Click += new System.EventHandler(this.button10_Click);
            //
            // button9
            //
            this.button9.Location = new System.Drawing.Point(324, 164);
            this.button9.Size = new System.Drawing.Size(100, 28);
            this.button9.TabIndex = 15;
            this.button9.Text = "Third state";
            this.button9.UseVisualStyleBackColor = true;
            this.button9.Click += new System.EventHandler(this.button9_Click);
            //
            // gbFan
            //
            this.gbFan.Controls.Add(this.fanAuto);
            this.gbFan.Controls.Add(this.fanFull);
            this.gbFan.Location = new System.Drawing.Point(12, 214);
            this.gbFan.Name = "gbFan";
            this.gbFan.Size = new System.Drawing.Size(436, 58);
            this.gbFan.TabIndex = 16;
            this.gbFan.TabStop = false;
            this.gbFan.Text = "CPU Fan";
            //
            // fanAuto
            //
            this.fanAuto.Location = new System.Drawing.Point(10, 22);
            this.fanAuto.Size = new System.Drawing.Size(100, 28);
            this.fanAuto.TabIndex = 17;
            this.fanAuto.Text = "Auto";
            this.fanAuto.UseVisualStyleBackColor = true;
            this.fanAuto.Click += new System.EventHandler(this.fanAuto_Click);
            //
            // fanFull
            //
            this.fanFull.Location = new System.Drawing.Point(116, 22);
            this.fanFull.Size = new System.Drawing.Size(100, 28);
            this.fanFull.TabIndex = 18;
            this.fanFull.Text = "Full";
            this.fanFull.UseVisualStyleBackColor = true;
            this.fanFull.Click += new System.EventHandler(this.fanFull_Click);
            //
            // gbKeyboard
            //
            this.gbKeyboard.Controls.Add(this.kbOff);
            this.gbKeyboard.Controls.Add(this.kbLow);
            this.gbKeyboard.Controls.Add(this.kbHigh);
            this.gbKeyboard.Location = new System.Drawing.Point(12, 280);
            this.gbKeyboard.Name = "gbKeyboard";
            this.gbKeyboard.Size = new System.Drawing.Size(436, 58);
            this.gbKeyboard.TabIndex = 19;
            this.gbKeyboard.TabStop = false;
            this.gbKeyboard.Text = "Keyboard Light";
            //
            // kbOff
            //
            this.kbOff.Location = new System.Drawing.Point(10, 22);
            this.kbOff.Size = new System.Drawing.Size(100, 28);
            this.kbOff.TabIndex = 20;
            this.kbOff.Text = "Off";
            this.kbOff.UseVisualStyleBackColor = true;
            this.kbOff.Click += new System.EventHandler(this.kbOff_Click);
            //
            // kbLow
            //
            this.kbLow.Location = new System.Drawing.Point(116, 22);
            this.kbLow.Size = new System.Drawing.Size(100, 28);
            this.kbLow.TabIndex = 21;
            this.kbLow.Text = "Low";
            this.kbLow.UseVisualStyleBackColor = true;
            this.kbLow.Click += new System.EventHandler(this.kbLow_Click);
            //
            // kbHigh
            //
            this.kbHigh.Location = new System.Drawing.Point(222, 22);
            this.kbHigh.Size = new System.Drawing.Size(100, 28);
            this.kbHigh.TabIndex = 22;
            this.kbHigh.Text = "High";
            this.kbHigh.UseVisualStyleBackColor = true;
            this.kbHigh.Click += new System.EventHandler(this.kbHigh_Click);
            //
            // gbStatus
            //
            this.gbStatus.Controls.Add(this.lblSensors);
            this.gbStatus.Controls.Add(this.lblBattery);
            this.gbStatus.Location = new System.Drawing.Point(12, 346);
            this.gbStatus.Name = "gbStatus";
            this.gbStatus.Size = new System.Drawing.Size(436, 62);
            this.gbStatus.TabIndex = 23;
            this.gbStatus.TabStop = false;
            this.gbStatus.Text = "System Status";
            //
            // lblSensors
            //
            this.lblSensors.AutoSize = false;
            this.lblSensors.Location = new System.Drawing.Point(12, 20);
            this.lblSensors.Name = "lblSensors";
            this.lblSensors.Size = new System.Drawing.Size(412, 18);
            this.lblSensors.Text = "CPU --°C      Fan --- RPM";
            //
            // lblBattery
            //
            this.lblBattery.AutoSize = false;
            this.lblBattery.Location = new System.Drawing.Point(12, 38);
            this.lblBattery.Name = "lblBattery";
            this.lblBattery.Size = new System.Drawing.Size(412, 18);
            this.lblBattery.Text = "Battery: --";
            //
            // gbOptions
            //
            this.gbOptions.Controls.Add(this.rememberKBD);
            this.gbOptions.Controls.Add(this.checkTurnKBLightOff);
            this.gbOptions.Location = new System.Drawing.Point(12, 416);
            this.gbOptions.Name = "gbOptions";
            this.gbOptions.Size = new System.Drawing.Size(436, 92);
            this.gbOptions.TabIndex = 24;
            this.gbOptions.TabStop = false;
            this.gbOptions.Text = "Options";
            //
            // rememberKBD
            //
            this.rememberKBD.Location = new System.Drawing.Point(10, 20);
            this.rememberKBD.Size = new System.Drawing.Size(420, 32);
            this.rememberKBD.TabIndex = 25;
            this.rememberKBD.Text = "Remember keyboard illumination level after restart, sleep, and other power events";
            this.rememberKBD.UseVisualStyleBackColor = true;
            this.rememberKBD.CheckedChanged += new System.EventHandler(this.rememberKBD_CheckedChanged);
            //
            // checkTurnKBLightOff
            //
            this.checkTurnKBLightOff.Location = new System.Drawing.Point(10, 54);
            this.checkTurnKBLightOff.Size = new System.Drawing.Size(420, 32);
            this.checkTurnKBLightOff.TabIndex = 26;
            this.checkTurnKBLightOff.Text = "Turn keyboard light off when applications go full screen (ideal for watching movies)";
            this.checkTurnKBLightOff.UseVisualStyleBackColor = true;
            this.checkTurnKBLightOff.CheckedChanged += new System.EventHandler(this.checkTurnKBLightOff_CheckedChanged);
            //
            // button1
            //
            this.button1.Location = new System.Drawing.Point(12, 516);
            this.button1.Size = new System.Drawing.Size(100, 28);
            this.button1.TabIndex = 27;
            this.button1.Text = "About";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            //
            // button4
            //
            this.button4.Location = new System.Drawing.Point(119, 516);
            this.button4.Size = new System.Drawing.Size(329, 28);
            this.button4.TabIndex = 28;
            this.button4.Text = "Register to run at system startup as admin";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            //
            // button12
            //
            this.button12.Location = new System.Drawing.Point(12, 553);
            this.button12.Size = new System.Drawing.Size(436, 28);
            this.button12.TabIndex = 29;
            this.button12.Text = "Choose driver";
            this.button12.UseVisualStyleBackColor = true;
            this.button12.Click += new System.EventHandler(this.button12_Click);
            //
            // NotifyIcon1
            //
            this.NotifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.NotifyIcon1.Text = "notifyIcon1";
            this.NotifyIcon1.Visible = true;
            this.NotifyIcon1.DoubleClick += new System.EventHandler(this.NotifyIcon1_DoubleClick);
            //
            // contextMenuStrip1
            //
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripSeparator1,
            this.toolStripMenuItem2});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(200, 62);
            //
            // toolStripMenuItem1
            //
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(199, 26);
            this.toolStripMenuItem1.Text = "Show application";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            //
            // toolStripSeparator1
            //
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(196, 6);
            //
            // toolStripMenuItem2
            //
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(199, 26);
            this.toolStripMenuItem2.Text = "Quit";
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            //
            // timer1
            //
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            //
            // lightTimer
            //
            this.lightTimer.Interval = 1000;
            this.lightTimer.Tick += new System.EventHandler(this.lightTimer_Tick);
            //
            // statusTimer
            //
            this.statusTimer.Interval = 2000;
            this.statusTimer.Tick += new System.EventHandler(this.statusTimer_Tick);
            //
            // tooltips
            //
            this.toolTip1.SetToolTip(this.powerOn, "Turn the power LED on");
            this.toolTip1.SetToolTip(this.powerOff, "Turn the power LED off");
            this.toolTip1.SetToolTip(this.powerBlink, "Blink the power LED (third state)");
            this.toolTip1.SetToolTip(this.dotOn, "Turn the red dot LED (on the 'i' in ThinkPad) on");
            this.toolTip1.SetToolTip(this.dotOff, "Turn the red dot LED off");
            this.toolTip1.SetToolTip(this.dotBlink, "Blink the red dot LED (third state)");
            this.toolTip1.SetToolTip(this.microphoneOn, "Turn the microphone-mute LED on");
            this.toolTip1.SetToolTip(this.microphoneOff, "Turn the microphone-mute LED off");
            this.toolTip1.SetToolTip(this.microphoneBlink, "Blink the microphone-mute LED (third state)");
            this.toolTip1.SetToolTip(this.sleepOn, "Turn the sleep (moon) LED on");
            this.toolTip1.SetToolTip(this.sleepOff, "Turn the sleep (moon) LED off");
            this.toolTip1.SetToolTip(this.sleepBlink, "Blink the sleep (moon) LED (third state)");
            this.toolTip1.SetToolTip(this.button11, "Turn the Fn-lock LED on");
            this.toolTip1.SetToolTip(this.button10, "Turn the Fn-lock LED off");
            this.toolTip1.SetToolTip(this.button9, "Blink the Fn-lock LED (third state)");
            this.toolTip1.SetToolTip(this.fanAuto, "Let the firmware control the fan automatically (normal)");
            this.toolTip1.SetToolTip(this.fanFull, "Run the fan at full speed for maximum cooling");
            this.toolTip1.SetToolTip(this.kbOff, "Turn the keyboard backlight off");
            this.toolTip1.SetToolTip(this.kbLow, "Set the keyboard backlight to low");
            this.toolTip1.SetToolTip(this.kbHigh, "Set the keyboard backlight to high");
            this.toolTip1.SetToolTip(this.button1, "About this application");
            this.toolTip1.SetToolTip(this.button4, "Create a scheduled task so the app starts at logon with admin rights (no UAC prompt)");
            this.toolTip1.SetToolTip(this.button12, "Choose the low-level driver used to access the hardware (WinRing0, TVicPort or PawnIO)");
            //
            // Form1
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(460, 591);
            this.Controls.Add(this.gbLeds);
            this.Controls.Add(this.gbFan);
            this.Controls.Add(this.gbKeyboard);
            this.Controls.Add(this.gbStatus);
            this.Controls.Add(this.gbOptions);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.button12);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "ThinkPad Toolbox";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.contextMenuStrip1.ResumeLayout(false);
            this.gbLeds.ResumeLayout(false);
            this.gbLeds.PerformLayout();
            this.gbFan.ResumeLayout(false);
            this.gbKeyboard.ResumeLayout(false);
            this.gbStatus.ResumeLayout(false);
            this.gbOptions.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.GroupBox gbLeds;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button powerOn;
        private System.Windows.Forms.Button powerOff;
        private System.Windows.Forms.Button powerBlink;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button dotOn;
        private System.Windows.Forms.Button dotOff;
        private System.Windows.Forms.Button dotBlink;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button microphoneOn;
        private System.Windows.Forms.Button microphoneOff;
        private System.Windows.Forms.Button microphoneBlink;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button sleepOn;
        private System.Windows.Forms.Button sleepOff;
        private System.Windows.Forms.Button sleepBlink;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Button button11;
        private System.Windows.Forms.Button button10;
        private System.Windows.Forms.Button button9;
        private System.Windows.Forms.GroupBox gbFan;
        private System.Windows.Forms.Button fanAuto;
        private System.Windows.Forms.Button fanFull;
        private System.Windows.Forms.GroupBox gbKeyboard;
        private System.Windows.Forms.Button kbOff;
        private System.Windows.Forms.Button kbLow;
        private System.Windows.Forms.Button kbHigh;
        private System.Windows.Forms.GroupBox gbStatus;
        private System.Windows.Forms.Label lblSensors;
        private System.Windows.Forms.Label lblBattery;
        private System.Windows.Forms.GroupBox gbOptions;
        private System.Windows.Forms.CheckBox rememberKBD;
        private System.Windows.Forms.CheckBox checkTurnKBLightOff;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.Button button12;
        private System.Windows.Forms.NotifyIcon NotifyIcon1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Timer lightTimer;
        private System.Windows.Forms.Timer statusTimer;
    }
}
