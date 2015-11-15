using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HabitTracker {
    public partial class Form2 : Form {
        private Form1 form1;

        public Form2(Form1 _f1) {
            InitializeComponent();
            form1 = _f1;
        }


        //btnResetRow_Click method
        //Purpose: Passes row number to form1.ResetRow
        //Paramenters:
        //Returns: None
        private void btnResetRow_Click(object sender, EventArgs e) {
            int rowNum;
            bool parseExecuted = int.TryParse(tbxResetRowNumber.Text, out rowNum); //Try to parse the RowNumber texbox into an integer.

            if (parseExecuted && rowNum != null && rowNum <= form1.GetRowCount() && rowNum > 0) //If the parse succeeded and the row number is valid: Update row text and save.
                form1.ResetRow(rowNum);
            else
                MessageBox.Show("Error: Failed to reset row. Check now number input.");
        }


        private void btnSwapRows_Click(object sender, EventArgs e) {
            int firstRow, secondRow;
            bool parse1Executed = int.TryParse(tbxSwapRow1.Text, out firstRow); //Try to parse the first row swap texbox into an integer.
            bool parse2Executed = int.TryParse(tbxSwapRow2.Text, out secondRow); //Try to parse the second row swap texbox into an integer.

            if (parse1Executed && firstRow <= form1.GetRowCount() && firstRow > 0 && parse2Executed && secondRow <= form1.GetRowCount() && secondRow > 0) //If the parse succeeded and the row numbers are valid...
                form1.SwapRows(firstRow, secondRow);
            else
                MessageBox.Show("Error: Failed to swap rows. Check inputs.");
        }


        //btnUpdateLabel_Click method
        //Purpose: Passes row number and new label to form1.UpdateLabel
        //Paramenters:
        //Returns: None
        private void btnUpdateLabel_Click(object sender, EventArgs e) {
            int rowNum;
            bool parseExecuted = int.TryParse(tbxEditRowLabelRowNumber.Text, out rowNum); //Try to parse the RowNumber texbox into an integer.
            string newLabel = tbxRowName.Text;

            if (parseExecuted && rowNum != null && rowNum <= form1.GetRowCount() && rowNum > 0) //If the parse succeeded and the row number is valid: Update row text and save.
                form1.UpdateLabel(rowNum, newLabel);
            else
                MessageBox.Show("Error: Failed to set row label. Check now number input.");
        }


        //btnUpdateRowCount_Click method
        //Purpose: Passes row count to form1.UpdateRowCount
        //Paramenters:
        //Returns: None
        private void btnUpdateRowCount_Click(object sender, EventArgs e) {
            int rowCount;
            bool parseExecuted = int.TryParse(tbxRowCount.Text, out rowCount); //Try to parse the RowCount texbox into an integer.

            if (parseExecuted && rowCount <= form1.GetRowCount() && rowCount > 0) //If the parse succeeded and the row number is valid: Update row text and save.
                form1.UpdateRowCount(rowCount);
            else
                MessageBox.Show("Error: Failed to set row label. Check row count input.");
        }


        


    }
}
