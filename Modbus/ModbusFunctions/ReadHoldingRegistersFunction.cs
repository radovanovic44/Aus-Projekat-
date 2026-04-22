using Common;
using Modbus.FunctionParameters;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read holding registers functions/requests.
    /// </summary>
    public class ReadHoldingRegistersFunction : ModbusFunction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadHoldingRegistersFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
        public ReadHoldingRegistersFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
        {
            CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
        }

        /// <inheritdoc />
        public override byte[] PackRequest()
        {
            var p = (ModbusReadCommandParameters)CommandParameters;

            byte functionCode = 3;

            byte[] request = new byte[12];

            ushort transactionId = p.TransactionId;
            byte unitId = p.UnitId;
            ushort startAddress = p.StartAddress;
            ushort quantity = p.Quantity;

            // MBAP header
            request[0] = (byte)(transactionId >> 8);
            request[1] = (byte)(transactionId);
            request[2] = 0;
            request[3] = 0;
            request[4] = 0;
            request[5] = 6;

            request[6] = unitId;
            request[7] = functionCode;

            request[8] = (byte)(startAddress >> 8);
            request[9] = (byte)(startAddress);

            request[10] = (byte)(quantity >> 8);
            request[11] = (byte)(quantity);

            return request;
        }
        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            Dictionary<Tuple<PointType, ushort>, ushort> retVal = new Dictionary<Tuple<PointType, ushort>, ushort>();
            ModbusReadCommandParameters p = (ModbusReadCommandParameters)CommandParameters;

            if (response[7] == ((byte)ModbusFunctionCode.READ_HOLDING_REGISTERS | 0x80))
            {
                HandeException(response[8]);
            }

            byte byteCount = response[8];

            for (int i = 0; i < p.Quantity; i++)
            {
                int dataIndex = 9 + i * 2;

                if (dataIndex + 1 >= 9 + byteCount)
                    break;

                ushort value = (ushort)((response[dataIndex] << 8) | response[dataIndex + 1]);
                ushort address = (ushort)(p.StartAddress + i);

                retVal.Add(new Tuple<PointType, ushort>(PointType.ANALOG_OUTPUT, address), value);
            }

            return retVal;
        }

    }
}