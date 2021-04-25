using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

public class Realm : IEquatable<Realm>
{
    public bool Equals([AllowNull] Realm other)
    {
        return true;
    }
}
