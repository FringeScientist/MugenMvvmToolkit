﻿namespace MugenMvvm.Binding.Constants
{
    public static class ParserComponentPriority
    {
        #region Fields

        public const int Constant = 1000;
        public const int Method = 990;
        public const int Indexer = 990;
        public const int Lambda = 990;
        public const int Member = 980;
        public const int Unary = 970;
        public const int Paren = 960;
        public const int Binary = 950;
        public const int Condition = 940;

        public const int TokenParser = 0;

        #endregion
    }
}