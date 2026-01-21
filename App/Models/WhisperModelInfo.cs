using System;
using System.Collections.Generic;
using System.Text;

namespace App.Models;

public class WhisperModelInfo
{
    public string ModelName { get; set; }
    public string ModelPath { get; set; }
    public string OriginalModelPath { get; set; }
    public long ModelSize { get; set; }
    public string ModelSizeFormatted { get; set; }
    public bool ModelExists { get; set; }
    public bool FactoryInitialized { get; set; }
    public int QuantizationLevel { get; set; }
    public string ModelType { get; set; }
}

