using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfApp1.Model
{
    public class Role : INotifyPropertyChanged
    {

        public int Id { get; set; }
        private string nameRole;
        public string NameRole { get; set; }

        public Role()
        {
            this.Persons = new HashSet<Person>();}

        public virtual ICollection<Person> Persons { get; set; }

        public Role(int id, string nameRole)
        {
            this.Id = id;
            this.NameRole = nameRole;
        }

        public Role ShallowCopy()
        {
            return (Role)this.MemberwiseClone();
        }


        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}