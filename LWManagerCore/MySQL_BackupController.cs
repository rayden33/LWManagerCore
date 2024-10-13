using LWManagerCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LWManagerCore
{
    public class MySQL_BackupController
    {
        private string _ServerUrl;
        private string _DatabaseName;
        private string _UserName;
        private string _Password;

        public MySQL_BackupController(string server, string databaseName, string userName, string password)
        {
            _ServerUrl = server;
            _DatabaseName = databaseName;
            _UserName = userName;
            _Password = password;

            using (var context = new LibraryContext())
            {
                context.Database.EnsureCreated();
                context.SaveChanges();
            }
        }


        public void Backup(ApplicationContext dbAc) 
        {
            using (var context = new LibraryContext())
            {
                /// 1. LeaseContracts
                var leaseContracts = dbAc.LeaseContracts;
                foreach (var item in leaseContracts)
                {
                    /// Check table values in backup mysql database and add if not exists
                    if (context.LeaseContracts.Where(p => p.Id == item.Id).Count() == 0)
                    {
                        context.LeaseContracts.Add(item);
                    }
                }
                context.SaveChanges();

                /// 2. ArchiveLeaseContracts
                var archiveLeaseContracts = dbAc.ArchiveLeaseContracts;
                foreach (var item in archiveLeaseContracts)
                {
                    /// Check table values in backup mysql database and add if not exists
                    if (context.ArchiveLeaseContracts.Where(p => p.Id == item.Id).Count() == 0)
                    {
                        context.ArchiveLeaseContracts.Add(item);
                    }
                }
                context.SaveChanges();

                /// 3. ReturnedLeaseContracts
                var returnedLeaseContracts = dbAc.ReturnedLeaseContracts;
                foreach (var item in returnedLeaseContracts)
                {
                    /// Check table values in backup mysql database and add if not exists
                    if (context.ReturnedLeaseContracts.Where(p => p.Id == item.Id).Count() == 0)
                    {
                        context.ReturnedLeaseContracts.Add(item);
                    }
                }
                context.SaveChanges();

                /// 4. Clients
                var clients = dbAc.Clients;
                foreach (var item in clients)
                {
                    /// Check table values in backup mysql database and add if not exists
                    if (context.Clients.Where(p => p.Id == item.Id).Count() == 0)
                    {
                        context.Clients.Add(item);
                    }
                }
                context.SaveChanges();

                /// 5. Products
                var products = dbAc.Products;
                foreach (var item in products)
                {
                    /// Check table values in backup mysql database and add if not exists
                    if (context.Products.Where(p => p.Id == item.Id).Count() == 0)
                    {
                        context.Products.Add(item);
                    }
                }
                context.SaveChanges();

                /// 6. ProductReferences
                var productReferences = dbAc.ProductReferences;
                foreach (var item in productReferences)
                {
                    /// Check table values in backup mysql database and add if not exists
                    if (context.ProductReferences.Where(p => p.Id == item.Id).Count() == 0)
                    {
                        context.ProductReferences.Add(item);
                    }
                }
                context.SaveChanges();

                /// 7. OrderProducts
                var orderProducts = dbAc.OrderProducts;
                foreach (var item in orderProducts)
                {
                    /// Check table values in backup mysql database and add if not exists
                    if (context.OrderProducts.Where(p => p.Id == item.Id).Count() == 0)
                    {
                        context.OrderProducts.Add(item);
                    }
                }
                context.SaveChanges();

                /// 8. ReturnedProducts
                var returnedProducts = dbAc.ReturnedProducts;
                foreach (var item in returnedProducts)
                {
                    /// Check table values in backup mysql database and add if not exists
                    if (context.ReturnedProducts.Where(p => p.Id == item.Id).Count() == 0)
                    {
                        context.ReturnedProducts.Add(item);
                    }
                }
                context.SaveChanges();

                /// 9. Payments
                var payments = dbAc.Payments;
                foreach (var item in payments)
                {
                    /// Check table values in backup mysql database and add if not exists
                    if (context.Payments.Where(p => p.Id == item.Id).Count() == 0)
                    {
                        context.Payments.Add(item);
                    }
                }
                context.SaveChanges();

            }
        }

        public void InsertData()
        {
            using(var context = new LibraryContext())
            {

                /// Check table values in backup mysql database
                
            }
        }
    }
}
