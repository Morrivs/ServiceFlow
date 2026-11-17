using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceFlow.Domain.Customers
{
    public class Customer
    {
        private Customer() { }

        public Customer(string fullName, string identificationNumber, string email, string phone, string? address = null)
        {
            FullName = fullName;
            IdentificationNumber = identificationNumber;
            Email = email;
            Phone = phone;
            Address = address;
            IsActive = true;
            CreatedAtUtc = DateTime.UtcNow;
        }

        public int Id { get; private set; }
        public int? UserId { get; private set; }
        public string FullName { get; private set; } = null!;
        public string IdentificationNumber { get; private set; } = null!;
        public string Email { get; private set; } = null!;
        public string Phone { get; private set; } = null!;
        public string? Address { get; private set; }
        public bool IsActive { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }
        public DateTime? UpdatedAtUtc { get; private set; }
        public byte[] RowVersion { get; private set; } = [];

        public void Deactivate()
        {
            IsActive = false;
            UpdatedAtUtc = DateTime.UtcNow;
        }
    }
}
