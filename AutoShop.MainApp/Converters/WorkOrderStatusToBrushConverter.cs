using AutoShop.Core.Enums;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AutoShop.MainApp.Converters;

public class WorkOrderStatusToBrushConverter : IValueConverter
{
    public object Convert(object value,
                          Type targetType,
                          object parameter,
                          CultureInfo culture)
    {
        if (value is not WorkOrderStatus status)
            return Brushes.Gray;

        return status switch
        {
            WorkOrderStatus.Draft => Brushes.Gray,
            WorkOrderStatus.Open => Brushes.DodgerBlue,
            WorkOrderStatus.InProgress => Brushes.DarkOrange,
            WorkOrderStatus.WaitingApproval => Brushes.Goldenrod,
            WorkOrderStatus.Completed => Brushes.Green,
            WorkOrderStatus.Paid => Brushes.DarkGreen,
            WorkOrderStatus.Closed => Brushes.Black,
            WorkOrderStatus.Cancelled => Brushes.Red,
            _ => Brushes.Gray
        };
    }

    public object ConvertBack(object value,
                              Type targetType,
                              object parameter,
                              CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}