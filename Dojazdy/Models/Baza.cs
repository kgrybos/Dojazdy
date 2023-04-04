using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using System.IO;

namespace Dojazdy.Models
{
    class Baza : IDisposable
    {
        private readonly string ConnectionString = @"User=SYSDBA;Password=masterkey;Database=C:\DOJAZDY.fdb;DataSource=localhost;Port=3050;
            Dialect=3;Charset=NONE;Role=;Connection lifetime = 15; Pooling=true;MinPoolSize=0;MaxPoolSize=50;Packet Size = 8192;ServerType=0;";
        private readonly FbConnection connection;
        bool disposed = false;

        public Baza()
        {
            connection = new FbConnection(ConnectionString);
            connection.Open();
        }

        public void DeletePrzystanki()
        {
            new FbCommand("DELETE FROM Przystanki", connection).ExecuteNonQuery();
            new FbCommand("SET GENERATOR gen_Przystanki_id TO 0;", connection).ExecuteNonQuery();
        }

        public void InsertPrzystanek(Przystanek doBazy)
        {
            using (FbCommand command = new FbCommand("INSERT INTO Przystanki (zespol, slupek, nazwa, lat, lon, obowiazujeod) VALUES (@zespol, @slupek, @nazwa, @lat, @lon, @obowiazujeod)", connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@zespol", doBazy.Id);
                command.Parameters.Add("@slupek", doBazy.Slupek);
                command.Parameters.Add("@nazwa", doBazy.Nazwa);
                command.Parameters.Add("@lat", doBazy.Lat);
                command.Parameters.Add("@lon", doBazy.Lon);
                command.Parameters.Add("@obowiazujeod", doBazy.ObowiazujeOd);
                command.ExecuteNonQuery();
            }
        }

        public List<Przystanek> SelectPrzystanki()
        {
            using (FbCommand command = new FbCommand("SELECT zespol, slupek, nazwa, lat, lon, obowiazujeod FROM Przystanki", connection))
            {
                command.CommandType = CommandType.Text;
                using (FbDataReader reader = command.ExecuteReader())
                {
                    List<Przystanek> nowy = new List<Przystanek>();
                    while (reader.Read())
                    {
                        nowy.Add(new Przystanek
                        {
                            Id = reader.GetString(0),
                            Slupek = reader.GetString(1),
                            Nazwa = reader.GetString(2),
                            Lat = reader.GetDouble(3),
                            Lon = reader.GetDouble(4),
                            ObowiazujeOd = reader.GetDateTime(5)
                        });
                    }
                    return nowy;
                }
            }
        }

        public void DeleteOdjazdy()
        {
            new FbCommand("DELETE FROM Odjazdy", connection).ExecuteNonQuery();
            new FbCommand("SET GENERATOR gen_Odjazdy_id TO 0;", connection).ExecuteNonQuery();
        }

        public bool doAktualizacji(Przystanek p)
        {
            int id = GetPrzystanekId(p);
            using (FbCommand command = new FbCommand("SELECT aktualizacja FROM Przystanki WHERE id=@id", connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@id", id);
                using (FbDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    if (reader.IsDBNull(0))
                    {
                        return true;
                    }
                    else
                    {
                        return reader.GetDateTime(0) < DateTime.Now.AddDays(-1);
                    }
                }
            }
        }

        public void UsunOdjazd(string linia, int idPrzystanku)
        {
            using (FbCommand command = new FbCommand("DELETE FROM Odjazdy WHERE linia=@linia AND idprzystanku=@idprzystanku", connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@linia", linia);
                command.Parameters.Add("@idprzystanku", idPrzystanku);
                command.ExecuteNonQuery();
            }
        }

        public void InsertOdjazd(Odjazd doBazy, int idPrzystanku)
        {
            using (FbCommand command = new FbCommand("INSERT INTO Odjazdy (czas, kierunek, linia, idprzystanku) VALUES (@czas, @kierunek, @linia, @idprzystanku)", connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@czas", doBazy.Czas);
                command.Parameters.Add("@kierunek", doBazy.Kierunek);
                command.Parameters.Add("@linia", doBazy.Linia);
                command.Parameters.Add("@idprzystanku", idPrzystanku);
                command.ExecuteNonQuery();
            }
        }

        public void InsertOdjazdy(List<Odjazd>[] doBazy, int idPrzystanku)
        {
            using (FbCommand command = new FbCommand("UPDATE Przystanki SET aktualizacja=@aktualizacja WHERE id=@id", connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@aktualizacja", DateTime.Now);
                command.Parameters.Add("@id", idPrzystanku);
                command.ExecuteNonQuery();
            }

            foreach (List<Odjazd> linia in doBazy)
            {
                UsunOdjazd(linia[0].Linia, idPrzystanku);
                foreach (Odjazd o in linia)
                {
                    InsertOdjazd(o, idPrzystanku);
                }
            }
        }

        public int GetPrzystanekId(Przystanek p)
        {
            using (FbCommand command = new FbCommand("SELECT id FROM Przystanki WHERE zespol=@zespol AND slupek=@slupek", connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@zespol", p.Id);
                command.Parameters.Add("@slupek", p.Slupek);
                using (FbDataReader reader = command.ExecuteReader())
                {
                    reader.Read();
                    return reader.GetInt32(0);
                }
            }
        }

        public Dictionary<string, List<DateTime>> SelectOdjazdy(int idPrzystanku) {
            using (FbCommand command = new FbCommand("SELECT czas, linia FROM Odjazdy WHERE idprzystanku=@idprzystanku", connection))
            {
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@idprzystanku", idPrzystanku);
                using (FbDataReader reader = command.ExecuteReader())
                {
                    Dictionary<string, List<DateTime>> nowy = new Dictionary<string, List<DateTime>>();
                    while(reader.Read())
                    {
                        string linia = reader.GetString(1);
                        if(!nowy.ContainsKey(linia))
                        {
                            nowy[linia] = new List<DateTime>();
                        }
                        nowy[linia].Add(reader.GetDateTime(0));
                    }
                    return nowy;
                }
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                connection.Dispose();
            }

            disposed = true;
        }
    }
}
