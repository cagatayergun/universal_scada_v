using System;
using System.Collections.Generic;

namespace TekstilScada.Models
{
    // Firma (Örn: A Tekstil, B İplik)
    public class Company
    {
        public int Id { get; set; }
        public string CompanyName { get; set; }
        public bool IsActive { get; set; }
    }

    // Fabrika (Gateway'in çalıştığı yer)
    public class Factory
    {
        public int Id { get; set; }
        public int CompanyId { get; set; } // Hangi firmaya ait?
        public string FactoryName { get; set; }

        // KRİTİK NOKTA: Bu Key, Gateway'deki App.config ile eşleşecek.
        // Bilgisayarı/Fabrikayı tanıyan tekil anahtar.
        public string ApiKey { get; set; }
    }

    // Makineye Fabrika ID eklentisi
    // (Mevcut Machine sınıfına bu property'i eklemelisiniz)
    /*
    public class Machine 
    {
        // ... diğer özellikler ...
        public int FactoryId { get; set; } 
    }
    */
}