using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static TollCollectionProject.VehicleRepository;

namespace TollCollectionProject
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            try
            {
                using (IRepository<Vehicle> vehicles = VehicleRepository.Instance)
                {
                    var vehicle1 = new Vehicle(id: 5, licensePlate: "Dhaka-Metro-1234", vehicleType: VehicleType.Car, tollPaid: 50);
                    vehicles.Add(vehicle1);

                    var vehicleToUpdate = vehicles.FindById(2);
                    vehicleToUpdate.TollPaid = 100;
                    vehicles.Update(vehicleToUpdate);
                    Console.WriteLine($"Vehicle {vehicleToUpdate.Id} updated successfully");
                    Console.WriteLine(vehicleToUpdate.ToString());

                    if (vehicles.Delete(vehicleToUpdate))
                        Console.WriteLine($"Vehicle {vehicleToUpdate.Id} deleted successfully");

                    var searchResult = vehicles.Search("Dhaka");
                    Console.WriteLine();
                    Console.WriteLine($"Total Vehicles found: {searchResult.Count()}");
                    Console.WriteLine("----------------------------------");

                    foreach (var v in searchResult)
                    {
                        Console.WriteLine(v.ToString());
                    }

                    var searchResultAsync = await vehicles.SearchAsync("Car");
                    Console.WriteLine();
                    Console.WriteLine($"Total Vehicles found asynchronously: {searchResultAsync.Count()}");
                    Console.WriteLine("----------------------------------");

                    foreach (var v in searchResultAsync)
                    {
                        Console.WriteLine(v.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                Console.ReadLine();
            }
        }
    }
    public enum VehicleType
    {
        Bus,
        Truck,
        Car,
        Motorcycle

    }

    public interface IBaseModel : IDisposable
    {
        int Id { get; }
        bool ValidateEntity();
    }

    public interface IRepository<T> : IDisposable, IEnumerable<T> where T : IBaseModel
    {
        IEnumerable<T> Data { get; }
        void Add(T entity);
        bool Delete(T entity);
        void Update(T entity);
        T FindById(int Id);
        IEnumerable<T> Search(string value);
        Task<IEnumerable<T>> SearchAsync(string value); // Async method
    }

    public sealed class Vehicle : IBaseModel
    {
        public int Id { get; }
        public string LicensePlate { get; set; }
        public VehicleType VehicleType { get; set; }
        public double TollPaid { get; set; }

        public Vehicle(int id, string licensePlate, VehicleType vehicleType, double tollPaid)
        {
            this.Id = id;
            this.LicensePlate = licensePlate;
            this.VehicleType = vehicleType;
            this.TollPaid = tollPaid;
        }

        public bool ValidateEntity()
        {
            return !string.IsNullOrWhiteSpace(LicensePlate) &&
                   // !string.IsNullOrWhiteSpace(VehicleType) &&
                   TollPaid >= 0;
        }

        public override string ToString()
        {
            return $"Vehicle Info\nVehicle ID : \t{this.Id}\nLicense Plate : \t{this.LicensePlate}\nVehicle Type : \t{this.VehicleType}\nToll Paid : \t{this.TollPaid}\n~~~~~~~~~~~~~~~~~~~~~~\n";
        }

        public void Dispose()
        {
        }
    }

    public sealed class VehicleRepository : IRepository<Vehicle>
    {
        private static VehicleRepository _instance = new VehicleRepository();
        private List<Vehicle> _data;

        public static VehicleRepository Instance
        {
            get
            {
                lock (_instance)
                {
                    return _instance;
                }
            }
        }

        private VehicleRepository()
        {
            _data = new List<Vehicle>
            {
                new Vehicle(1, "Dhaka-Metro-1111", VehicleType.Bus, 100),
                new Vehicle(2, "Chittagong-4567", VehicleType.Truck, 200),
                new Vehicle(3, "Khulna-7890",VehicleType. Car , 50),
                new Vehicle(4, "Rajshahi-1234", VehicleType.Motorcycle, 20)
            };
        }

        public void Dispose()
        {
            _data.Clear();
        }

        public IEnumerable<Vehicle> Data => _data;

        public Vehicle this[int index]
        {
            get
            {
                return _data[index];
            }
        }

        public void Add(Vehicle entity)
        {
            if (_data.Any(d => d.Id == entity.Id))
            {
                throw new Exception("Duplicate vehicle ID, try another");
            }
            else if (entity.ValidateEntity())
            {
                _data.Add(entity);
            }
            else
            {
                throw new Exception("Vehicle is invalid");
            }
        }

        public bool Delete(Vehicle entity)
        {
            return _data.Remove(entity);
        }

        public void Update(Vehicle entity)
        {
            int preIdx = _data.FindIndex(d => d.Id == entity.Id);
            _data[preIdx] = entity;
        }

        public Vehicle FindById(int Id)
        {
            var result = _data.FirstOrDefault(d => d.Id == Id);
            return result;
        }

        public async Task<IEnumerable<Vehicle>> SearchAsync(string value)
        {
            var result = await Task.Run(() =>
            {
                return from d in _data.AsParallel()
                       where d.Id.ToString().Equals(value) ||
                             d.LicensePlate.Contains(value) ||
                             d.VehicleType.ToString().StartsWith(value, StringComparison.OrdinalIgnoreCase) ||
                             d.TollPaid.ToString().Contains(value)
                       orderby d.LicensePlate ascending
                       select d;
            });

            return result;
        }

        public IEnumerable<Vehicle> Search(string value)
        {
            var result = from d in _data.AsParallel()
                         where d.Id.ToString().Contains(value) ||
                               d.LicensePlate.Contains(value) ||
                               d.VehicleType.ToString().StartsWith(value, StringComparison.OrdinalIgnoreCase) ||
                               d.TollPaid.ToString().Contains(value)
                         orderby d.LicensePlate ascending
                         select d;

            return result;
        }


        public IEnumerator<Vehicle> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }
    }
}
