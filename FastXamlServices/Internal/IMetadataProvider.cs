﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FastXamlServices.Internal
{
	interface IMetadataProvider
	{
		Action<SerializationWriterContext, object> GetWriter(Type type);
		Func<SerializationReaderContext, object> GetReader(Type type);
	}
}
