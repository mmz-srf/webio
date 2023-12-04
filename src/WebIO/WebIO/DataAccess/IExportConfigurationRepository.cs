namespace WebIO.DataAccess;

using System.Collections.Generic;
using Model.Display;

public interface IExportConfigurationRepository {
    List<ColumnDefinition> BfeBom { get; }
    List<ColumnDefinition> BfeIo { get; }
    List<ColumnDefinition> NevionBom { get; }
    List<ColumnDefinition> NevionIo { get; }
    List<ColumnDefinition> Nevion { get; }
}