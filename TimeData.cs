using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonitorSystem
{
    public enum DataState
    {
        New,
        Active,
        Submitted
    }

    public enum TimeEntryType
    {
        TelephoneCall,
        Research,
        DraftingDocument
    }
    public class TimeData:ICloneable
    {
        #region properties
        public DataState State { get; set; }
        public string Title { get; set; }
        public TimeEntryType EntryType { get; set; }

        public double HourlyRate { get; set; }
        public double Total
        {
            get
            {
                var min = Duration.TimeOfDay.Minutes;
                if (min > 0)
                {
                    if (min < 15)
                    {
                        min = 15;
                    }
                    else if (min < 30)
                    {
                        min = 30;
                    }
                    else if (min < 45)
                    {
                        min = 45;
                    }
                    else if (min < 60)
                    {
                        min = 60;
                    }
                }
                var timespan = new TimeSpan(Duration.TimeOfDay.Days,Duration.TimeOfDay.Hours,min,0);
                return Math.Round(timespan.TotalMinutes / 60 * HourlyRate, 2);
            }
        }

        private DateTime hour;
        public DateTime Duration  //this is my mapped property
        {
            get
            {
                return hour;
            }
            set
            {
                hour = value;
                //update unmapped property 
                hourString = value.ToString("HH:mm");
            }
        }

        private string hourString;
        public string HourString
        {
            get
            {
                return hourString;
            }
            set
            {
                var time = value.Split(':');
                int hr;
                int min;
                var parse = int.TryParse(time[0], out hr);
                var parseMin = int.TryParse(time[1], out min);
                if(parse && hr>23)
                {
                    parse = false;
                }
                if(parseMin && min>59)
                {
                    parseMin = false;
                }
                if(hr==24 && min!=0)
                {
                    parse = false;
                }
                if (parse && parseMin)
                {
                    Duration = new DateTime(hour.Year, hour.Month, hour.Day, Convert.ToInt16(time[0]), Convert.ToInt16(time[1]), 0);
                }
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
