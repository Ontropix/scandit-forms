using System;
using System.Threading.Tasks;

namespace Code9
{
	public interface IScanner
	{
		Task<ScanResult> ScanAsync ();
		void Configure(string licenseKey, string cancelText);
	}
}

