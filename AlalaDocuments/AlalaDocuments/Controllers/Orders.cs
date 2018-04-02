﻿using System;
using System.Linq;
using System.Runtime.InteropServices;

using SAPbobsCOM;

using AlalaDiConnector.Controllers;
using AlalaDocuments.Models;

namespace AlalaDocuments.Controllers
{
    public class Orders : Interfaces.IOrders
    {
        private readonly Company _company;

        public Orders(DiConnectionController connection)
        {
            _company = connection.Company;
        }

        public OrderModel GetById(int docEntry)
        {
            // Prepare the object
            var orderObj = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

            OrderModel order = null;
            if (orderObj.GetByKey(docEntry))
            {
                order = new OrderModel();
                order.DocEntry = orderObj.DocEntry;
                order.BusinessPartner = orderObj.CardCode;

                // TODO: Add code to retrieve line data of the order.
            }

            Marshal.ReleaseComObject(orderObj);
            return order;
        }

        public void Create(OrderModel order)
        {
            // Prepare the object
            var orderObj = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

            // Set header values
            orderObj.CardCode = order.BusinessPartner;
            orderObj.DocDueDate = DateTime.Now;
            orderObj.BPL_IDAssignedToInvoice = 1;

            // Set line values
            foreach (var item in order.ItemList)
            {
                if (item != order.ItemList.First())
                {
                    orderObj.Lines.Add();
                }

                orderObj.Lines.ItemCode = item.ItemCode;
                orderObj.Lines.Quantity = item.Quantity;
            }

            // Add it to database
            var success = orderObj.Add().Equals(0);
            if (!success)
            {
                // Error handling
                int code;
                string msg;
                _company.GetLastError(out code, out msg);
                throw new Exception($"Something went wrong\n{code} {msg}");
            }

            Marshal.ReleaseComObject(orderObj);
        }

        public bool UpdateItems(int docEntry, OrderModel order)
        {
            // Prepare the object
            var orderObj = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

            var orderFound = false;
            if (orderObj.GetByKey(docEntry))
            {
                orderFound = true;

                foreach (var item in order.ItemList)
                {
                    if (orderObj.Lines.Count > 0)
                    {
                        orderObj.Lines.Add();
                    }

                    orderObj.Lines.ItemCode = item.ItemCode;
                    orderObj.Lines.Quantity = item.Quantity;
                    orderObj.Lines.TaxCode = item.TaxCode;
                    orderObj.Lines.AccountCode = item.AccountCode;
                }

                var success = orderObj.Update().Equals(0);
                if (!success)
                {
                    // Error handling
                    int code;
                    string msg;
                    _company.GetLastError(out code, out msg);
                    throw new Exception($"Something went wrong\n{code} {msg}");
                }
            }

            Marshal.ReleaseComObject(orderObj);
            return orderFound;
        }

        public bool Delete(int docEntry)
        {
            // Prepare the object
            var orderObj = (Documents)_company.GetBusinessObject(BoObjectTypes.oOrders);

            var orderFound = false;
            if (orderObj.GetByKey(docEntry))
            {
                orderFound = true;

                // Remove it from database
                var success = orderObj.Remove().Equals(0);
                if (!success)
                {
                    // Error handling
                    int code;
                    string msg;
                    _company.GetLastError(out code, out msg);
                    throw new Exception($"Something went wrong\n{code} {msg}");
                }
            }

            Marshal.ReleaseComObject(orderObj);
            return orderFound;
        }
    }
}
