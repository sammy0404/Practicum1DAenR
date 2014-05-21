using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace QueryVerwerker
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            GUI GUI = new GUI();
            Application.Run(GUI);
        }
    }


    class GUI : Form
    {
        TextBox invoer = new TextBox();
        TextBox uitvoer = new TextBox();
        Font letters = new Font("Arial", 10);
        Brush kleur = new SolidBrush(Color.Black);
        Pen iets = new Pen(Color.Black);
        Button ok = new Button();
        Button clipboard = new Button();
        Button exporteren = new Button();
        Button importeren = new Button();

        public GUI()
        {
            //window
            Size s = new Size(800, 600);
            this.Size = s;
            this.MaximumSize = s;
            this.MinimumSize = s;
            this.MaximizeBox = false;

            //invoertext
            invoer.Size = new Size(500, 50);
            invoer.ScrollBars = ScrollBars.Vertical;
            invoer.Multiline = true;
            invoer.Location = new Point(100, 120);
            this.Controls.Add(invoer);

            //ok button            
            ok.Text = "OK";
            ok.Size = new Size(50, 50);
            ok.Location = new Point(650, 120);
            this.Controls.Add(ok);
            ok.Click += handleOK;

            //uitvoertext
            uitvoer.Location = new Point(100, 200);
            uitvoer.Size = new Size(500, 300);
            uitvoer.Multiline = true;
            uitvoer.ScrollBars = ScrollBars.Vertical;
            this.Controls.Add(uitvoer);

            //clipboard
            clipboard.Location = new Point(100, 510);
            clipboard.Size = new Size(200, 50);
            clipboard.Text = "Copy to clipboard";
            clipboard.Click += handleClipBoard;
            this.Controls.Add(clipboard);

            //Exporteren






            //Importeren




            //daadwerkelijk uitvoeren
            this.Paint += MainPaintMethod;
        }
        private void MainPaintMethod(object obj, PaintEventArgs pea)
        {
            pea.Graphics.DrawString("Welkom op onze schitterende applicatie", letters, kleur, new Point(250, 250));
            pea.Graphics.DrawString("Voer hieronder uw query in", letters, kleur, new PointF(100, 100));




        }
        private void handleOK(object obj, EventArgs ea)
        {
            string[] query = invoer.Text.Split(';');
            StringBuilder b = new StringBuilder();
            for (int i = 0; i < query.Length - 1; i++)
            {
                string q = query[i];
                b.AppendLine("Ingevoerde query: " + q);
                b.AppendLine("Resultaat: ");
                QueryHandler handler = new QueryHandler(q);

                b.AppendLine("-----------------------------------------------------------");

            }
            uitvoer.Text = b.ToString();
        }
        private void handleClipBoard(object obj, EventArgs ea)
        {
            if (uitvoer.Text == "")
            {
                Clipboard.SetText("Aangezien de uitvoer leeg was, hebben wij deze schitterende tekst geschreven zodat u zich herinnert dat Gerben en Sam een 10 verdienen");
            }
            else
            {
                Clipboard.SetText(uitvoer.Text);
            }
        }
    }
}
