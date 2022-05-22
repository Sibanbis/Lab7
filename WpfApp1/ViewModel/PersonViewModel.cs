using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using WpfApp1.View;
using WpfApp1.Helper;
using Newtonsoft.Json;

namespace WpfApp1.Model
{
    public class PersonViewModel : INotifyPropertyChanged
    {
        private RoleViewModel vmRole = new RoleViewModel();
        readonly string path = @"C:\Users\Днс\source\repos\WpfApp1\WpfApp1\DataModels\PersonData.json";

        //private PersonDPO _selectedPersonDPO;
        ///// <summary>
        ///// выделенные в списке данные по сотруднику 
        ///// </summary>
        //public PersonDPO SelectedPersonDPO
        //{
        //    get { return _selectedPersonDPO; }
        //    set
        //    {
        //        _selectedPersonDPO = value;
        //        OnPropertyChanged("SelectedPersonDPO");
        //    }
        //}
        ///// <summary>
        /// коллекция данных по сотрудникам
        /// </summary>
        public ObservableCollection<Person> ListPerson { get; set; }
        //public ObservableCollection<PersonDPO> ListPersonDPO
        //{
        //    get;
        //    set;
        //}
        string _jsonPersons = String.Empty;
        public string Error { get; set; }
        public string Message { get; set; }
        public PersonViewModel()
        {
            ListPerson = new ObservableCollection<Person>();
            ListPerson = GetPersons();
        }

        private ObservableCollection<Person> GetPersons()
        {
            using (var context = new CompanyEntities())
            {
                var query = from per in context.Persons
                        .Include("Role")
                    orderby per.LastName
                    select per;
                if (query.Count() != 0)
                {
                    foreach (var p in query)
                    {
                        ListPerson.Add(p);
                    }
                }
            }
            return ListPerson;
        }


        #region AddPerson
        /// <summary>
        /// добавление сотрудника
        /// </summary>
        private RelayCommand _addPerson;
        /// <summary>
        /// добавление сотрудника
        /// </summary>
        public RelayCommand AddPerson
        {
            get
            {
                return _addPerson ??
                       (_addPerson = new RelayCommand(obj =>
                       {
                           Person newPerson = new Person
                           {
                               Birthday = DateTime.Now
                           };
                           WindowNewEmployee wnPerson = new WindowNewEmployee
                           {
                               Title = "Новый сотрудник",
                               DataContext = newPerson
                           };
                           wnPerson.ShowDialog();
                           if (wnPerson.DialogResult == true)
                           {
                               using (var context = new CompanyEntities())
                               {
                                   try
                                   {
                                       Person ord = context.Persons.Add(newPerson);
                                       context.SaveChanges();
                                       ListPerson.Clear();
                                       ListPerson = GetPersons();
                                   }
                                   catch (Exception ex)
                                   {
                                       MessageBox.Show("\nОшибка добавления данных!\n" +
                                                       ex.Message, "Предупреждение");
                                   }
                               }
                           }
                       }, (obj) => true));
            }
        }
        #endregion

        #region EditPerson
        /// команда редактирования данных по сотруднику
        private RelayCommand _editPerson;
        public RelayCommand EditPerson
        {
            get
            {
                return _editPerson ??
                       (_editPerson = new RelayCommand(obj =>
                       {
                           Person editPerson = SelectedPerson;
                           
                           WindowNewEmployee wnPerson = new WindowNewEmployee()
                           {
                               Title = "Редактирование данных сотрудника",
                               DataContext = editPerson
                           };
                           wnPerson.ShowDialog();
                           if (wnPerson.DialogResult == true)
                           {
                               using (var context = new CompanyEntities())
                               {
                                   Person person = context.Persons.Find(editPerson.Id);
                                   if (person != null)
                                   {
                                       if (person.RoleId != editPerson.RoleId)
                                           person.RoleId = editPerson.RoleId;
                                       if (person.FirstName != editPerson.FirstName)
                                           person.FirstName = editPerson.FirstName;
                                       if (person.LastName != editPerson.LastName)
                                           person.LastName = editPerson.LastName;
                                       if (person.Birthday != editPerson.Birthday)
                                           person.Birthday = editPerson.Birthday;
                                       try
                                       {
                                           context.SaveChanges();
                                           ListPerson.Clear();
                                           ListPerson = GetPersons();
                                       }
                                       catch (Exception ex)
                                       {
                                           MessageBox.Show("\nОшибка редактирования данных!\n"
                                                           + ex.Message, "Предупреждение");
                                       }
                                   }
                               }
                           }
                           else
                           {
                               ListPerson.Clear();
                               ListPerson = GetPersons();
                           }
                       }, (obj) => SelectedPerson != null && ListPerson.Count >
                           0));
            }
        }
        #endregion

        #region DeletePerson
        /// команда удаления данных по сотруднику
        private RelayCommand _deletePerson;
        public RelayCommand DeletePerson
        {
            get
            {
                return _deletePerson ??
                       (_deletePerson = new RelayCommand(obj =>
                       {
                           Person delPerson = SelectedPerson;
                           using (var context = new CompanyEntities())
                           {
                               // Поиск в контексте удаляемого автомобиля
                               Person person = context.Persons.Find(delPerson.Id);
                               if (person != null)
                               {
                                   MessageBoxResult result = MessageBox.Show("Удалить данные по сотруднику: \nФамилия: " + person.LastName + 
            
                                   "\nИмя: " + person.FirstName,
                                   "Предупреждение", MessageBoxButton.OKCancel);
                                   if (result == MessageBoxResult.OK)
                                   {
                                       try
                                       {
                                           context.Persons.Remove(person);
                                           context.SaveChanges();
                                           ListPerson.Remove(delPerson);
                                       }
                                       catch (Exception ex)
                                       {
                                           MessageBox.Show("\nОшибка удаления данных!\n" +
                                                           ex.Message, "Предупреждение");
                                       }
                                   }
                               }
                           }
                       }, (obj) => SelectedPerson != null && ListPerson.Count >
                           0));
            }
        }

        public Person SelectedPerson { get; private set; }
        #endregion


        #region Method
        /// <summary>
        /// Загрузка данных по сотрудникам из json файла
        /// </summary>
        /// <returns></returns>
        public ObservableCollection<Person> LoadPerson()
                                   {
                                       _jsonPersons = File.ReadAllText(path);
                                       if (_jsonPersons != null)
                                       {
                                           ListPerson = JsonConvert.DeserializeObject<ObservableCollection<Person>>(_jsonPersons);
                                           return ListPerson;
                                       }
                                       else
                                       {
                                           return null;
                                       }
                                   }
                                   /// <summary>
                                   /// Формирование коллекции классов PersonDpo из коллекции Person
                                   /// </summary>
                                   /// <returns></returns>
                                   //public ObservableCollection<PersonDPO> GetListPersonDPO()
                                   //{
                                   //    foreach (var person in ListPerson)
                                   //    {
                                   //        PersonDPO p = new PersonDPO();
                                   //        p = p.CopyFromPerson(person);
                                   //        ListPersonDPO.Add(p);
                                   //    }
                                   //    return ListPersonDPO;
                                   //}
                                   /// <summary>
                                   /// Нахождение максимального Id в коллекции данных
                                   /// </summary>
                                   /// <returns></returns>
                                   public int MaxId()
                                   {
                                       int max = 0;
                                       foreach (var r in this.ListPerson)
                                       {
                                           if (max < r.Id)
                                           {
                                               max = r.Id;
                                           };
                                       }
                                       return max;
                                   }
                                   /// <summary>
                                   /// Сохранение json-строки с данными по сотрудникам в jsonфайл
                                   /// </summary>
                                   /// <param name="listPersons"></param>
                                   private void SaveChanges(ObservableCollection<Person> listPersons)
                                   {
                                       var jsonPerson = JsonConvert.SerializeObject(listPersons);
                                       try
                                       {
                                           using (StreamWriter writer = File.CreateText(path))
                                           {
                                               writer.Write(jsonPerson);
                                           }
                                       }
                                       catch (IOException e)
                                       {
                                           Error = "Ошибка записи json файла /n" + e.Message;
                                       }
                                   }
        #endregion
        public event PropertyChangedEventHandler PropertyChanged;
        // [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName]
                        string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

//        private PersonDPO selectedPersonDPO;
//        public PersonDPO SelectedPersonDPO
//        {
//            get { return selectedPersonDPO; }
//            set
//            {
//                Console.WriteLine(value);
//                selectedPersonDPO = value;
//                OnPropertyChanged("SelectedPersonDPO");
//            }
//        }

//        public ObservableCollection<Person> ListPerson
//        {
//            get;
//            set;
//        } = new ObservableCollection<Person>();
//        public ObservableCollection<PersonDPO> ListPersonDPO
//        {
//            get;
//            set;
//        } = new ObservableCollection<PersonDPO>();

//        public PersonViewModel(RoleViewModel vmRole)
//        {
//            this.vmRole = vmRole;

//            this.ListPerson.Add(
//            new Person
//            {
//                Id = 1,
//                RoleId = 1,
//                FirstName = "Иван",
//                LastName = "Иванов",
//                Birthday = new DateTime(1980, 02, 28)
//            });
//            this.ListPerson.Add(
//                new Person
//                {
//                    Id = 2,
//                    RoleId = 2,
//                    FirstName = "Петр",
//                    LastName = "Петров",
//                    Birthday = new DateTime(1981, 03, 20)
//                }
//            );

//            this.ListPerson.Add(
//            new Person
//            {
//                Id = 3,
//                RoleId = 3,
//                FirstName = "Виктор",
//                LastName = "Викторов",
//                Birthday = new DateTime(1982, 04, 15)
//            });
//            this.ListPerson.Add(
//            new Person
//            {
//                Id = 4,
//                RoleId = 3,
//                FirstName = "Сидор",
//                LastName = "Сидоров",
//                Birthday = new DateTime(1983, 05, 10)
//            });
//            ListPersonDPO = GetListPersonDPO();
//        }

//        public ObservableCollection<PersonDPO> GetListPersonDPO()
//        {
//            foreach (var person in ListPerson)
//            {
//                PersonDPO p = new PersonDPO();
//                p = p.CopyFromPerson(person);
//                ListPersonDPO.Add(p);
//            }
//            return ListPersonDPO;
//        }

//        public int MaxId()
//        {
//            int max = 0;
//            foreach (var r in this.ListPerson)
//            {
//                if (max < r.Id)
//                {
//                    max = r.Id;
//                };
//            }
//            return max;
//        }

//        #region AddPerson
//        private RelayCommand addPerson;
//        public RelayCommand AddPerson
//        {
//            get
//            {
//                return addPerson ??
//                (addPerson = new RelayCommand(obj =>
//                {
//                    WindowNewEmployee wnPerson = new WindowNewEmployee
//                    {
//                        Title = "Новый сотрудник"
//                    };
//                    // формирование кода нового собрудника
//                    int maxIdPerson = MaxId() + 1;
//                    PersonDPO per = new PersonDPO
//                    {
//                        Id = maxIdPerson,
//                        Birthday = DateTime.Now
//                    };
//                    wnPerson.DataContext = per;
//                    wnPerson.CbRole.ItemsSource = vmRole.ListRole;
//                    if (wnPerson.ShowDialog() == true)
//                    {
//                        Role r = (Role)wnPerson.CbRole.SelectedValue;
//                        per.RoleName = r.NameRole;
//                        per.Role = r.NameRole;
//                        ListPersonDPO.Add(per);
//                        // добавление нового сотрудника в коллекцию ListPerson<Person> 
//                        Person p = new Person();
//                        p = p.CopyFromPersonDPO(per);
//                        ListPerson.Add(p);
//                    }
//                },
//                (obj) => true));
//            }
//        }
//        #endregion
//        #region EditPerson
//        /// команда редактирования данных по сотруднику
//        private RelayCommand editPerson;
//        public RelayCommand EditPerson
//        {
//            get
//            {
//                return editPerson ??
//                (editPerson = new RelayCommand(obj =>
//                {
//                    WindowNewEmployee wnPerson = new WindowNewEmployee()
//                    {
//                        Title = "Редактирование данных сотрудника",
//                    };
//                    PersonDPO personDPO = SelectedPersonDPO;
//                    PersonDPO tempPerson = new PersonDPO();
//                    tempPerson = personDPO.ShallowCopy();
//                    wnPerson.DataContext = tempPerson;
//                    wnPerson.CbRole.ItemsSource = vmRole.ListRole;
//                    FindPerson finder = new FindPerson(personDPO.Id);
//                    List<Person> listPerson = ListPerson.ToList();
//                    Person p = listPerson.Find(new Predicate<Person>(finder.PersonPredicate));
//                    FindRole roleFinder = new FindRole(p.RoleId);
//                    Role role = vmRole.ListRole.ToList().Find(new Predicate<Role>(roleFinder.RolePredicate));
//                    wnPerson.CbRole.SelectedItem = role;

//                    if (wnPerson.ShowDialog() == true)
//                    {
//                         // сохранение данных в оперативной памяти
//                         // перенос данных из временного класса в класс отображения 
//                         // данных 
//                        Role r = (Role)wnPerson.CbRole.SelectedValue;
//                        personDPO.RoleName = r.NameRole;
//                        personDPO.Role = r.NameRole;
//                        personDPO.FirstName = tempPerson.FirstName;
//                        personDPO.LastName = tempPerson.LastName;
//                        personDPO.Birthday = tempPerson.Birthday;
//                        // перенос данных из класса отображения данных в класс Person
//                        p.CopyFromPersonDPO(personDPO);
//                        p.RoleId = r.Id;
//                    }
//                }, (obj) => SelectedPersonDPO != null && ListPersonDPO.Count> 0));
//            }
//        }

//        #endregion
//        #region DeletePerson 
//        // команда удаления данных по сотруднику
//        private RelayCommand deletePerson;

//        public event PropertyChangedEventHandler PropertyChanged;

//        public RelayCommand DeletePerson
//        {
//            get
//            {
//                return deletePerson ??
//                (
//                    deletePerson = new RelayCommand(
//                        obj => {
//                            PersonDPO person = SelectedPersonDPO;
//                            MessageBoxResult result = MessageBox.Show("Удалить данные по сотруднику: \n" + person.LastName + " " + person.FirstName, "Предупреждение", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
//                            if (result == MessageBoxResult.OK)
//                            {
//                             // удаление данных в списке отображения данных
//                            ListPersonDPO.Remove(person);
//                            // удаление данных в списке классов ListPerson<Person>
//                            Person per = new Person();
//                                 per = per.CopyFromPersonDPO(person);
//                                 ListPerson.Remove(per);
//                            }
//                        }, 
//                        obj => SelectedPersonDPO != null && ListPersonDPO.Count > 0
//                    )
//                );
//            }
//        }

//        #endregion
//        //public event PropertyChangedEventHandler PropertyChanged;
//        //[NotifyPropertyChangedInvocator]
//        //protected virtual void OnPropertyChanged([CallerMemberName]
//        //    string propertyName = "")
//        //{
//        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        //}

//        protected virtual void OnPropertyChanged(
//            [CallerMemberName] string propertyName = ""
//        ) {
//            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
//        }
//    }

//}

