using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class OuterDataModel
{
	//Also contains meta deta, if we want to check
	public DataModel datamodel;
}

class DataModel
{
	public SessionConfigData config;
	public SessionStateData save;
    public Dictionary<string, List<string>> assignment;
}

