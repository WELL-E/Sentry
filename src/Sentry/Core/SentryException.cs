﻿using System;

namespace Sentry.Core
{
    public class SentryException : Exception
    {
        public SentryException()
        {
        }

        public SentryException(string message) : base(message)
        {
        }

        public SentryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}