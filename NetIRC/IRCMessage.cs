﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetIRC
{
    public class IRCMessage
    {
        public string Raw { get; }

        private string prefix;
        private string command;
        private string[] parameters;

        public string Prefix => prefix;
        public string Command => command;
        public string[] Parameters => parameters;

        public IRCMessage(string rawData)
        {
            Raw = rawData;
            Parse(rawData);
        }

        private void Parse(string rawData)
        {
            var indexOfNextSpace = 0;

            if (RawDataHasPrefix)
            {
                indexOfNextSpace = rawData.IndexOf(' ');
                prefix = rawData.Substring(1, indexOfNextSpace - 1);
            }

            if (RawDataDoesNotContainSpaces)
            {
                command = rawData;
                return;
            }

            indexOfNextSpace = rawData.IndexOf(' ');
            command = rawData.Remove(indexOfNextSpace);
            rawData = rawData.Substring(indexOfNextSpace + 1);

            parameters = new[] { rawData };
        }

        public bool RawDataHasPrefix => Raw.StartsWith(":");

        public bool RawDataDoesNotContainSpaces => !Raw.Contains(" ");
    }
}
