﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace VisualLog.Core
{
  public class JLog
  {
    public List<JMessage> Messages { get; set; }
    public Encoding Encoding { get; private set; }
    public string SourceFilePath { get; private set; }

    #region ctors

    public JLog()
    { 
      this.Messages = new List<JMessage>();
    }

    public JLog(string sourceFilePath) : this()
    {
      this.SourceFilePath = sourceFilePath;
    }
    public JLog(string path, Encoding encoding) : this(path)
    {
      this.Encoding = encoding;
    }

    #endregion

    public void Read()
    {
      if (this.Encoding != null)
        this.Messages.AddRange(File.ReadAllLines(this.SourceFilePath, this.Encoding).Select(x => new JMessage(x)));
      else
        this.Messages.AddRange(File.ReadAllLines(this.SourceFilePath).Select(x => new JMessage(x)));
    }

    public void Parse()
    {
      foreach (var message in this.Messages)
        message.TryParse();
    }

    public void Save()
    {
      File.WriteAllLines(this.SourceFilePath, this.Messages.Select(x => JsonConvert.SerializeObject(x.JsonObject)));
    }

    public IEnumerable<JMessage> GetBetween(DateTime? from, DateTime? to)
    {
      return this.Messages.Where(x => x.MessageDateTimeBetween(from, to));
    }

    public IEnumerable<JMessage> GetByTrace(string[] trace)
    {
      return this.Messages.Where(x => trace.Contains(x.Trace));
    }

    public void OrderMessagesByDateTime()
    {
      this.Messages = this.Messages.OrderBy(x => x.DateTime).ToList();
    }

    public void CalculateTimeDelta()
    {
      JMessage previousMessage = null;
      foreach (var message in this.Messages)
      {
        message.AddTimeDeltaPropertyFirst();
        if (previousMessage != null)
          message.CalculateTimeDeltaFrom(previousMessage);
        previousMessage = message;
      }
    }
  }
}
