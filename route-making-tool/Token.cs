using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BusDriverFile {
    class Token {
        private ulong value;
        public Token(ulong value) {
            this.value=value;
        }
        public Token(string text) {
            value=StringToToken(text);
        }
        public Token(BinaryReader br) {
            value=br.ReadUInt64();
        }

        public string Text { get => getString(value); set => this.value=StringToToken(value); }
        public ulong Value { get => value; set => this.value=value; }

        public static readonly char[] CharacterSet =
        { '\0', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b',
            'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o',
            'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '_'
        };
        private static readonly int CharsetLength = CharacterSet.Length; // =38
        public static readonly int MaxLength = 12;

        /// <summary>
        /// Converts a string to a token.
        /// </summary>
        /// <param name="input">The input string</param>
        /// <returns>A token.</returns>
        public static ulong StringToToken(string input) {
            if(string.IsNullOrEmpty(input)) return 0;
            if(!IsToken(input))
                throw new ArgumentException($"Input is not a valid token.");

            var token = 0UL;
            for(var i = 0;i < input.Length;i++) {
                token += (ulong)Math.Pow(CharsetLength,i) *
                    (ulong)GetCharIndex(input[i]);
            }
            return token;
        }

        /// <summary>
        /// Converts a token to string.
        /// </summary>
        /// <param name="token">The token</param>
        /// <returns>The input string</returns>
        public static string TokenToString(ulong token) {
            // for empty tokens, return "" rather than "\0"
            if(token == 0) return "";

            // Determine length of string
            int length = 1;
            while(Math.Pow(CharsetLength,length) - 1 < token) {
                length++;
            }

            // reverse the token, from last to first character
            var input = new char[length];
            for(var i = length;i > 0;i--) {
                // find the last character of the input
                // by dividing by 38^len and ignoring the remainder
                var pow = (ulong)Math.Pow(CharsetLength,i - 1);
                var character = token / pow;
                input[i - 1] += CharacterSet[character];

                // subtract that part from the token
                token -= character * pow;
            }

            var inputStr = new string(input);
            return inputStr;
        }

        private static readonly byte[] charset = new byte[] {0,//0号位占位
            48,49,50,51,52,53,54,55,56,57,           //0-9
            97,98,99,100,101,102,103,104,105,106,    //a-j
            107,108,109,110,111,112,113,114,115,116, //k-t
            117,118,119,120,121,122,                 //u-z
            95 //下划线
        };

        public static string getString(ulong num) {
            byte[] buffer = new byte[12];
            byte i = 0;
            while(num>38) {
                buffer[i++]=charset[num%38];
                num/=38;
            }
            buffer[i]=charset[num];
            return Encoding.ASCII.GetString(buffer,0,i+1);
        }

        public override string ToString() {
            return Text;
        }

        /// <summary>
        /// Returns the index of a character.
        /// </summary>
        /// <param name="letter">The character.</param>
        /// <returns>Its index.</returns>
        static int GetCharIndex(char letter) {
            var index = Array.IndexOf(CharacterSet,letter);
            return index == -1 ? 0 : index;
        }

        public static bool IsToken(string str) {
            if(str.Length > MaxLength) return false;

            foreach(var c in str) {
                if(Array.IndexOf(CharacterSet,c) == -1)
                    return false;
            }
            return true;
        }
    }
}
