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
using System.Globalization;

namespace HabitTracker {
    public partial class Form1 : Form {
        Button[] rowButtons;
        Label[] rowLabels, rowNumbers;
        GroupBox[] rowBoxes;
        Label[] toDoLabels;

        public const int ROW_BUTTON_COUNT = 310, ROW_COUNT = 10;
        const string fileName = @"htdata.txt";
        const string fileNameToDo = @"todo.txt";
        
        bool pauseButtonSave = false;
        int rowsInUse = ROW_COUNT;


        public Form1() {
            InitializeComponent();
        }


        //Form1_Load method
        //Purpose: Initialize reference variables and loads label, button, and toDo information.
        //Paramenters:
        //Returns: None
        private void Form1_Load(object sender, EventArgs e) {
            rowButtons = new Button[ROW_BUTTON_COUNT];
            for (int i = 0; i < ROW_BUTTON_COUNT; i++) //Initialize references to checkbox objects on the form.
                rowButtons[i] = (Button)this.Controls.Find("rowBtn" + (i + 1).ToString(), true)[0];

            rowLabels = new Label[ROW_COUNT];
            for (int i = 0; i < ROW_COUNT; i++) //Initialize references to row name label objects on the form.
                rowLabels[i] = (Label)this.Controls.Find("lblRow" + (i + 1).ToString(), true)[0];

            rowNumbers = new Label[ROW_COUNT];
            for (int i = 0; i < ROW_COUNT; i++) //Initialize references to row number label objects on the form.
                rowNumbers[i] = (Label)this.Controls.Find("lblRowNum" + (i + 1).ToString(), true)[0];

            rowBoxes = new GroupBox[ROW_COUNT];
            for (int i = 0; i < ROW_COUNT; i++) //Initialize references to row groupBox objects on the form.
                rowBoxes[i] = (GroupBox)this.Controls.Find("gbButtonRow" + (i + 1).ToString(), true)[0];

            toDoLabels = new Label[ROW_COUNT];
            for (int i = 0; i < ROW_COUNT; i++) //Initialize references to todo label objects on the form.
                toDoLabels[i] = (Label)this.Controls.Find("lblToDo" + (i + 1).ToString(), true)[0];

            if (File.Exists(fileName)) { //If the file exists...
                LoadButtonsFromFile();
            }
            else //Else, file does not exist: Only update month/year label on the form.
                lblMonthYear.Text = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month) + " " + DateTime.Now.Year;

            LoadToDoList();
        }


        //aboutToolStripMeuItem_Click method
        //Purpose: Displays about information.
        //Paramenters:
        //Returns: None
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e) {
            MessageBox.Show("Habit Tracker v.0.3b\nCreated by Aaron Tholl\nAugust 20, 2015");
        }


        //editRowsToolStripMenuItem_Click method
        //Purpose: Displays the row editor form.
        //Paramenters:
        //Returns: None
        private void editRowsToolStripMenuItem_Click(object sender, EventArgs e) {
            Form2 f2 = new Form2(this);
            f2.Show();
        }


        //exitToolStripMenuItem_Click method
        //Purpose: Exits the program.
        //Paramenters:
        //Returns: None
        private void exitToolStripMenuItem_Click(object sender, EventArgs e) {
            this.Close();
        }


        //resetFormToolStripMenuItem1_Click method
        //Purpose: Allows user to reset all checkboxes and rows.
        //Paramenters:
        //Returns: None
        private void resetFormToolStripMenuItem1_Click(object sender, EventArgs e) {
            var confirmReset = MessageBox.Show("Are you sure you wish to reset the form? All boxes and rows will be reset.\n(You can reset individual rows in the row editor under Tools.)", "Confirm Reset", MessageBoxButtons.YesNo); //Show confirmation dialog box.

            if (confirmReset == DialogResult.Yes) { //If user confirmed: Uncheck every checkbox, reset every row label, and save.
                pauseButtonSave = true;

                rowsInUse = ROW_COUNT;
                ToggleRowVisibility();

                foreach (Button btn in rowButtons)
                    btn.BackColor = Color.White;

                int rowNumFixed = 0;
                foreach (Label lbl in rowLabels) {
                    lbl.Text = "Row " + (rowNumFixed + 1);
                    rowNumFixed++;
                }

                pauseButtonSave = false;
                SaveButtonsToFile();
            }
        }


        //anyRowBtn_MouseClicked method
        //Purpose: Changes button color and saves.
        //Paramenters:
        //Returns: None
        void anyRowBtn_MouseClicked(Object sender, MouseEventArgs e) {
            Button clickedButton = (Button)sender;

            if (Control.ModifierKeys == Keys.Control) { //If the user is holding the control key...
                if (clickedButton.BackColor != Color.Gainsboro)
                    clickedButton.BackColor = Color.Gainsboro;
                else
                    clickedButton.BackColor = Color.White;
            } 
            else {
                if (clickedButton.BackColor == Color.White)
                    clickedButton.BackColor = Color.Green;
                else if (clickedButton.BackColor == Color.Green)
                    clickedButton.BackColor = Color.Red;
                else if (clickedButton.BackColor == Color.Red)
                    clickedButton.BackColor = Color.Gold;
                else if (clickedButton.BackColor == Color.Gold)
                    clickedButton.BackColor = Color.White;
            } 

            SaveButtonsToFile();
        }


        //anyToDoLabel_MouseClicked method
        //Purpose: Clears to do item if control button is held while item is clicked.
        //Paramenters:
        //Returns: None
        void anyToDoLabel_MouseClicked(Object sender, MouseEventArgs e) {
            Label clickedLabel = (Label)sender;

            if (Control.ModifierKeys == Keys.Control) { //If the user is holding the control key...
                clickedLabel.Text = string.Empty;

                //Shift up
                for (int i = 1; i < 10; i++) {
                    if (toDoLabels[i].Text != string.Empty && toDoLabels[i - 1].Text == string.Empty) {
                        toDoLabels[i - 1].Text = toDoLabels[i].Text;
                        toDoLabels[i].Text = string.Empty;
                    }
                }
            }

            SaveToDoToFile();
        }


        //GetRowCount method
        //Purpose: Returns ROW_COUNT
        //Paramenters: None
        //Returns: ROW_COUNT(int)
        public int GetRowCount() {
            return ROW_COUNT;
        }


        //LoadButtonsFromFile method
        //Purpose: Load month, year, label, and button state information from a file.
        //Paramenters: None
        //Returns: None
        void LoadButtonsFromFile() {
            lblStatus.Text = "Loading";

            using (StreamReader file = new StreamReader(fileName)) { //Creates a StreamReader, closes the StreamReader once finished.
                string line, month = string.Empty, year = string.Empty;
                int boxCount = 0, rowCount = 0;

                if ((line = file.ReadLine()) != null)
                    month = line; //month gets first line of the file
                if ((line = file.ReadLine()) != null)
                    year = line; //year gets second line of the file.
                if ((line = file.ReadLine()) != null) {
                    int.TryParse(line, out rowsInUse); //Try to convert third line of file to int and save to rowsInUse
                    if (rowsInUse != ROW_COUNT)
                        ToggleRowVisibility();
                }

                while (rowCount < ROW_COUNT && (line = file.ReadLine()) != null) { //While we haven't read all the row labels yet and there is a line to read: Save next row label as next line.
                    rowLabels[rowCount].Text = line;
                    rowCount++;
                }

                string currentMonthYear = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month) + " " + DateTime.Now.Year; //currentMonthYear gets current month name and current year.
                if ((month + " " + year) != currentMonthYear) { //If the file's month/year don't coincide with the current month/year: Save our data file with a different name, create a new data file, and stop loading.
                    file.Close();

                    string backupFileName = @"htdata" + month + year + ".txt";
                    if (File.Exists(backupFileName))
                        File.Delete(backupFileName);
                    File.Copy(fileName, backupFileName);

                    lblMonthYear.Text = currentMonthYear;

                    SaveButtonsToFile();
                    return;
                } else //Else, month/year coincide: Update month/year label on the form.
                    lblMonthYear.Text = month + " " + year;

                while (file.Peek() >= 0) { //While there are more characters in the file: Set the color of the button according to the next letter in the file.
                    char[] c = new char[1];
                    file.Read(c, 0, c.Length);

                    if (c[0] == 'w') {
                        rowButtons[boxCount].BackColor = Color.White;
                        boxCount++;
                    } else if (c[0] == 'g') {
                        rowButtons[boxCount].BackColor = Color.Green;
                        boxCount++;
                    } else if (c[0] == 'r') {
                        rowButtons[boxCount].BackColor = Color.Red;
                        boxCount++;
                    } else if (c[0] == 'o') {
                        rowButtons[boxCount].BackColor = Color.Gold;
                        boxCount++;
                    } else if (c[0] == 'd') {
                        rowButtons[boxCount].BackColor = Color.Gainsboro;
                        boxCount++;
                    }
                }
            }
            lblStatus.Text = string.Empty; //Reset status label text.
        }


        //LoadToDoList method
        //Purpose: Load toDo information from a file.
        //Paramenters: None
        //Returns: None
        void LoadToDoList() {
            if (File.Exists(fileNameToDo)) { //If the file exists...
                lblStatus.Text = "Loading";
                using (StreamReader file = new StreamReader(fileNameToDo)) { //Creates a StreamReader, closes the StreamReader once finished.
                    string line;
                    int lineCount = 0;

                    while (lineCount < 10 && (line = file.ReadLine()) != null) { //While we haven't read all the toDo labels yet and there is a line to read...
                        toDoLabels[lineCount].Text = line;
                        lineCount++;
                    }

                }
                lblStatus.Text = string.Empty; //Reset status label text.
            }
        }


        //ResetRow method
        //Purpose: Reset specific row's label and set all its buttons to white.
        //Paramenters: 
        //Returns: None
        public void ResetRow(int rowNum) {
            pauseButtonSave = true;

            int rowNumFixed = rowNum - 1;

            rowLabels[rowNumFixed].Text = "Row " + rowNum;

            for (int i = 0; i < 31; i++)
                rowButtons[(rowNumFixed * 31) + i].BackColor = Color.White;

            pauseButtonSave = false;
            SaveButtonsToFile();
        }


        //SaveToDoToFile method
        //Purpose: Save toDo information to a file.
        //Paramenters: None
        //Returns: None
        void SaveToDoToFile() {
            lblStatus.Text = "Saving";

            try {
                using (StreamWriter file = new StreamWriter(fileNameToDo)) { //Creates a StreamWriter, closes the streamwriter once finished.
                    for (int i = 0; i < 10; i++) { //Write each row's to do text to a file.
                        file.WriteLine(toDoLabels[i].Text);
                    }
                }
            }
            catch (IOException e) {
                MessageBox.Show("There was a problem saving your last entry. Try not to submit so fast.");
            }

            lblStatus.Text = string.Empty; //Reset status label text.
        }


        //SaveToFile method
        //Purpose: Save label and button information to a file.
        //Paramenters: None
        //Returns: None
        void SaveButtonsToFile() {
            if (pauseButtonSave)
                return;

            lblStatus.Text = "Saving";

            try {
                using (StreamWriter file = new StreamWriter(fileName)) { //Creates a StreamWriter, closes the streamwriter once finished.
                    file.WriteLine(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(DateTime.Now.Month)); //Write current month name to file.
                    file.WriteLine(DateTime.Now.Year); //Write current year to file.
                    file.WriteLine(rowsInUse); //Write rowsInUse to file.
                    for (int i = 0; i < ROW_COUNT; i++) //Write each row's name in order.
                        file.WriteLine(rowLabels[i].Text);

                    for (int i = 0; i < ROW_BUTTON_COUNT; i++) { //Write each button's state in order.
                        if (rowButtons[i].BackColor == Color.White)
                            file.Write("w");
                        else if (rowButtons[i].BackColor == Color.Green)
                            file.Write("g");
                        else if (rowButtons[i].BackColor == Color.Red)
                            file.Write("r");
                        else if (rowButtons[i].BackColor == Color.Gold)
                            file.Write("o");
                        else if (rowButtons[i].BackColor == Color.Gainsboro)
                            file.Write("d");
                    }
                }
            } catch (IOException e) {
                MessageBox.Show("There was a problem saving your last change. (You are a fast clicker!)");
            }

            lblStatus.Text = string.Empty; //Reset status label text.
        }


        //SwapRows method
        //Purpose: Swap the labels and button colors of two rows.
        //Paramenters: 
        //Returns: None
        public void SwapRows(int firstRow, int secondRow) {
            pauseButtonSave = true;

            int firstRowFixed = firstRow - 1;
            int secondRowFixed = secondRow - 1;
            
            string tempLabel = rowLabels[firstRowFixed].Text;
            rowLabels[firstRowFixed].Text = rowLabels[secondRowFixed].Text;
            rowLabels[secondRowFixed].Text = tempLabel;

            for (int i = 0; i < 31; i++) {
                Color tempButtonColor = rowButtons[(firstRowFixed * 31) + i].BackColor;
                rowButtons[(firstRowFixed * 31) + i].BackColor = rowButtons[(secondRowFixed * 31) + i].BackColor;
                rowButtons[(secondRowFixed * 31) + i].BackColor = tempButtonColor;
            }

            pauseButtonSave = false;
            SaveButtonsToFile();
        }


        //tbxAddToDo_KeyDown method
        //Purpose: Checks if the key was a return key, adds the text in the input to the toDo list, and saves the toDo list to a file.
        //Paramenters: 
        //Returns: None
        private void tbxAddToDo_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Return) {
                string newText = tbxAddToDo.Text;
                tbxAddToDo.Text = string.Empty;

                for (int i = 0; i < 10; i++) {
                    if (toDoLabels[i].Text == string.Empty) {
                        toDoLabels[i].Text = newText;
                        break;
                    }
                }

                SaveToDoToFile();
            }
        }


        //ToggleRowVisibility method
        //Purpose: Toggles rows visible/invisible if they are in use/not in use.
        //Paramenters:
        //Returns: None
        void ToggleRowVisibility() {
            for (int i = 0; i < ROW_COUNT; i++) {
                if ((i + 1) <= rowsInUse) {
                    rowLabels[i].Visible = true;
                    rowNumbers[i].Visible = true;
                    rowBoxes[i].Visible = true;
                } 
                else {
                    rowLabels[i].Visible = false;
                    rowNumbers[i].Visible = false;
                    rowBoxes[i].Visible = false;
                }
            }
        }


        //UpdateLabel method
        //Purpose: Updates row with new label.
        //Paramenters: Row number, new label.
        //Returns: None
        public void UpdateLabel(int rowNum, string newLabel) {
            rowLabels[rowNum - 1].Text = newLabel;
            SaveButtonsToFile();
        }


        //UpdateRowCount method
        //Purpose: Updates rowsInUse.
        //Paramenters:
        //Returns: None
        public void UpdateRowCount(int rowCount) {
            rowsInUse = rowCount;

            ToggleRowVisibility();
            SaveButtonsToFile();
        }

    }
}
