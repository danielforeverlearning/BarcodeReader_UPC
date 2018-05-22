using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;

namespace BarcodeReader_UPC
{
    class Program
    {
        static void Main(string[] args)
        {
            string input_file     = @".\UPC_A_sample1.jpg";
            Bitmap img            = (Bitmap)Image.FromFile(input_file, true);

            double image_width    = 2592.0;
            int    center_YY      = 972;
            double O_sub_L        = 171.0;
            double O_sub_R        = 2396.0;
            double est_base_width = (O_sub_R - O_sub_L) / 95.0;

            // 1 == white
            //-1 == black

            //startguard = 3 * (est_base_width)
            //digit      = 7 * (est_base_width)
            //Left side of middle guard has 6 digits

            double est_digit_width     = (7.0) * (est_base_width);
            int[] est_left_digit_start = new int[6];
            int[] est_left_digit_end   = new int[6];
            for (int jj=0; jj <= 5; jj++)
            {
                double O_sub_j = O_sub_L + ((3.0) * (est_base_width)) + (est_digit_width * ((double)jj));
                est_left_digit_start[jj] = (int) O_sub_j;
            }

            for (int jj=1; jj <= 5; jj++)
            {
                est_left_digit_end[jj - 1] = est_left_digit_start[jj] - 1;
            }
            est_left_digit_end[5] = (int)((double)est_left_digit_start[5] + est_digit_width);

            
            /****************************************
            CALCULATE deformable templates
            *****************************************/
            double[] muh_black = new double[6];
            double[] muh_white = new double[6];
            double[] mean      = new double[6];
            double[] variance  = new double[6];
            double[] max_value = new double[6];
            double[] min_value = new double[6];
            for (int jj=0; jj <= 5; jj++)
            {
                int start_pixel_XX = est_left_digit_start[jj];
                int end_pixel_XX   = est_left_digit_end[jj];
                int half_count     = (end_pixel_XX - start_pixel_XX) / 2;
                List<double> digit_pixels = new List<double>();
                max_value[jj] = 0;
                min_value[jj] = 5000;
                for (int xx=start_pixel_XX; xx <= end_pixel_XX; xx++)
                {
                    Color pv = img.GetPixel(xx, center_YY);
                    int temp = pv.R + pv.G + pv.B;
                    double myvalue = ((double)temp / 3.0);
                    digit_pixels.Add(myvalue);

                    if (myvalue < min_value[jj])
                        min_value[jj] = myvalue;

                    if (myvalue > max_value[jj])
                        max_value[jj] = myvalue;
                }

                int cc = 0;
                double half_sum = 0;
                double total_sum = 0;
                foreach (double dd in digit_pixels.OrderBy(d => d))
                {
                    cc++;
                    half_sum += dd;
                    if (cc == half_count)
                    {
                        muh_black[jj] = half_sum / half_count;
                        total_sum = half_sum;
                        cc = 0;
                        half_sum = 0;
                    }
                }
                muh_white[jj] = half_sum / cc;
                total_sum += half_sum;
                mean[jj] = total_sum / ((double)digit_pixels.Count);

                //calculate variance
                double var_sum = 0;
                foreach (double dd in digit_pixels)
                {
                    double temp = dd - mean[jj];
                    temp = temp * temp;
                    var_sum += temp;
                }
                variance[jj] = var_sum / (double)digit_pixels.Count;
            }

            Console.WriteLine("DONE");


        }//Main
    }//class
}//namespace
