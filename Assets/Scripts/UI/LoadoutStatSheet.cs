using System;
using System.Collections.Generic;
using System.Globalization;
using PolishTanks.Tank.Modules;
using UnityEngine;

namespace PolishTanks.UI
{
    /// <summary>
    /// The stats a playtester compares when swapping modules, as text rows with the change since the previous build.
    /// Each stat knows whether more is better (armor) or worse (mass, reload), so the row can say which way it went.
    /// </summary>
    public static class LoadoutStatSheet
    {
        private const float Epsilon = 0.0005f;

        private readonly struct Entry
        {
            public Entry(string label, Func<TankStats, float> read, string format, string unit, bool higherIsBetter)
            {
                Label = label;
                Read = read;
                Format = format;
                Unit = unit;
                HigherIsBetter = higherIsBetter;
            }

            public string Label { get; }
            public Func<TankStats, float> Read { get; }
            public string Format { get; }
            public string Unit { get; }
            public bool HigherIsBetter { get; }
        }

        public static List<StatLine> Build(TankStats stats, TankStats previous, float kmhPerUnit)
        {
            var lines = new List<StatLine>();
            if (stats == null)
                return lines;

            foreach (var entry in Entries(kmhPerUnit))
            {
                var value = entry.Read(stats);
                var text = Format(value, entry.Format) + entry.Unit;
                if (previous == null)
                {
                    lines.Add(new StatLine(entry.Label, text, string.Empty, StatChange.Same));
                    continue;
                }

                var delta = value - entry.Read(previous);
                if (Mathf.Abs(delta) <= Epsilon * Mathf.Max(1f, Mathf.Abs(value)))
                {
                    lines.Add(new StatLine(entry.Label, text, string.Empty, StatChange.Same));
                    continue;
                }

                var better = delta > 0f == entry.HigherIsBetter;
                var deltaText = (delta > 0f ? "+" : "−") + Format(Mathf.Abs(delta), entry.Format);
                lines.Add(new StatLine(entry.Label, text, deltaText, better ? StatChange.Better : StatChange.Worse));
            }
            return lines;
        }

        private static IEnumerable<Entry> Entries(float kmhPerUnit)
        {
            yield return new Entry("Masa", s => s.TotalMass / 1000f, "0.0", " t", false);
            yield return new Entry("Moc na tonę", s => s.PowerToWeight, "0.0", " KM/t", true);
            yield return new Entry("Prędkość", s => s.Drive.maxForwardSpeed * kmhPerUnit, "0", " km/h", true);
            yield return new Entry("Przyspieszenie", s => s.Drive.acceleration, "0.0", "", true);
            yield return new Entry("Skręt kadłuba", s => s.Drive.turnRate, "0", "°/s", true);
            yield return new Entry("Obrót wieży", s => s.Turret.maxTraverseSpeed, "0", "°/s", true);
            yield return new Entry("Przeładowanie", s => s.Gun.reloadTime, "0.00", " s", false);
            yield return new Entry("Penetracja", s => (s.PenetrationMin + s.PenetrationMax) * 0.5f, "0", " mm", true);
            yield return new Entry("Obrażenia", s => s.GunDamage, "0", "", true);
            yield return new Entry("Rozrzut", s => s.GunDispersion, "0.00", "°", false);
            yield return new Entry("Pancerz przód", s => s.FrontArmor, "0", " mm", true);
            yield return new Entry("Pancerz bok", s => s.SideArmor, "0", " mm", true);
            yield return new Entry("Pancerz tył", s => s.RearArmor, "0", " mm", true);
            yield return new Entry("Wieża przód", s => s.TurretFrontArmor, "0", " mm", true);
            yield return new Entry("Punkty życia", s => s.HitPoints, "0", "", true);
            yield return new Entry("Pole widzenia", s => s.ViewRange, "0.0", "", true);
            yield return new Entry("Amunicja", s => s.AmmoCapacity, "0", "", true);
            yield return new Entry("Koszt", s => s.Cost, "0", "", false);
        }

        private static string Format(float value, string format) => value.ToString(format, CultureInfo.InvariantCulture);
    }
}
