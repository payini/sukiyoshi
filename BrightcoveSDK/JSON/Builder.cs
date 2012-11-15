namespace BrightcoveSDK.JSON
{
   /// <summary>
   /// RJE 10-16-2012
   /// Helper class for building JSON objects.  
   /// The class supports chaining so you can do stuff like this:
   /// string json = new Builder()
   ///      .Append("{")
   ///         .AppendField("MyStringField", "blah").Append(",")
   ///         .AppendField("MyLongField", 1000).Append(",")
   ///         .AppendField("MyBoolField", true)
   ///      .Append("}").ToString()
   /// </summary>
   public class Builder
   {
      System.Text.StringBuilder sb = new System.Text.StringBuilder();

      public override string ToString()
      {
         return sb.ToString();
      }

      public Builder Append(string s)
      {
         sb.Append(s);
         return this;
      }

      public Builder AppendFormat(string format, params object[] paramList)
      {
         return Append(string.Format(format, paramList));
      }

      public Builder AppendObject(string key, string value)
      {
         // Note: no quotes on value is intentional
         return AppendFormat("\"{0}\": {1}", key, value);
      }

      public Builder AppendField(string key, string value)
      {
         return AppendFormat("\"{0}\": \"{1}\"", key, value);
      }

      public Builder AppendField(string key, long value)
      {
         return AppendFormat("\"{0}\": \"{1}\"", key, value);
      }

      public Builder AppendField(string key, bool value)
      {
         return AppendFormat("\"{0}\": \"{1}\"", key, value ? "true" : "false");
      }
   }
}