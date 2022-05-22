using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WpfApp1.Model;

namespace WpfApp1.View
{

    public partial class WindowRole : Window
    {
        private RoleViewModel vmRole;
        private PersonViewModel vmPerson;

        public WindowRole(RoleViewModel vmRole)
        {
            InitializeComponent();
            this.vmRole = vmRole;
            DataContext = vmRole;
            lvRole.ItemsSource = vmRole.ListRole;
        }

        private void LvRole_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            vmRole.SelectedRole = (Role) e.AddedItems[0];
        }
    }
}
