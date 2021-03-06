﻿using System;
using System.IO;

namespace SL.RExcel.XLS.Records
{
    public class RkRecord : RowColXfCellRecord
    {
        public RkRecord(BIFFData data, Stream stream)
        {
            BinaryReader reader = new BinaryReader(data.ToStream());
            SetRowColXf(reader);
            Value = Convet(reader.ReadInt32());
        }

        public static double Convet(int rk)
        {
            var result = 0.0;

            bool div100 = (rk & 0x01) != 0;
            bool isInteger = (rk & 0x02) != 0;

            if (!isInteger)
            {
                result = ToDouble(rk & ~1);
                if (div100)
                    result /= 100.0;
            }
            else
            {
                rk = rk >> 2;
                result = div100 ? (rk / 100.0) : rk;
            }
            return result;
        }

        private static double ToDouble(int n)
        {
            byte[] doubleBytes = new byte[8];
            byte[] uintBytes = BitConverter.GetBytes(n);
            Array.Copy(uintBytes, 0, doubleBytes, doubleBytes.Length - uintBytes.Length, uintBytes.Length);

            return BitConverter.ToDouble(doubleBytes, 0);
        }
    }
}