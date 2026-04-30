using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

// Token: 0x0200022B RID: 555
public class CSVReader : IDisposable
{
	// Token: 0x0600091F RID: 2335 RVA: 0x000351A1 File Offset: 0x000333A1
	public CSVReader(string path, char separator = ',')
		: this(new FileStream(path, FileMode.Open, FileAccess.Read), separator)
	{
	}

	// Token: 0x06000920 RID: 2336 RVA: 0x000351B2 File Offset: 0x000333B2
	public CSVReader(Stream stream, char separator = ',')
		: this(new StreamReader(stream), separator)
	{
	}

	// Token: 0x06000921 RID: 2337 RVA: 0x000351C4 File Offset: 0x000333C4
	public CSVReader(TextReader reader, char separator = ',')
	{
		this.separator = separator;
		List<string> list = new List<string>();
		string text;
		while (this.ReadCSVLine(reader, out text))
		{
			list.Add(text);
		}
		this.rows = list.ToArray();
		reader.Dispose();
	}

	// Token: 0x06000922 RID: 2338 RVA: 0x00035214 File Offset: 0x00033414
	private bool ReadCSVLine(TextReader reader, out string result)
	{
		StringBuilder stringBuilder = new StringBuilder();
		string text = reader.ReadLine();
		if (text != null)
		{
			text = text.Replace("\\\"\"", "\\\"");
			bool flag = this.OpenQuote(false, text);
			if (flag)
			{
				stringBuilder.AppendLine(text);
			}
			else
			{
				stringBuilder.Append(text);
			}
			while (flag && text != null)
			{
				text = reader.ReadLine();
				if (text != null)
				{
					flag = this.OpenQuote(flag, text);
					if (flag)
					{
						stringBuilder.AppendLine(text);
					}
					else
					{
						stringBuilder.Append(text);
					}
				}
			}
		}
		result = stringBuilder.ToString();
		return text != null;
	}

	// Token: 0x06000923 RID: 2339 RVA: 0x0003529C File Offset: 0x0003349C
	private bool OpenQuote(bool openQuote, string line)
	{
		for (int i = 0; i < line.Length; i++)
		{
			if (line[i] == '"' && (i == 0 || line[i - 1] != '\\'))
			{
				openQuote = !openQuote;
			}
		}
		return openQuote;
	}

	// Token: 0x06000924 RID: 2340 RVA: 0x000352DC File Offset: 0x000334DC
	public List<List<string>> ReadToEnd()
	{
		List<List<string>> list = new List<List<string>>();
		for (List<string> list2 = this.ReadRow(); list2 != null; list2 = this.ReadRow())
		{
			list.Add(list2);
		}
		return list;
	}

	// Token: 0x06000925 RID: 2341 RVA: 0x0003530A File Offset: 0x0003350A
	public void Rewind()
	{
		this.activeIdx = 0;
	}

	// Token: 0x06000926 RID: 2342 RVA: 0x00035314 File Offset: 0x00033514
	public List<string> ReadRow()
	{
		List<string> list = new List<string>();
		if (this.activeIdx < 0 || this.activeIdx >= this.rows.Length)
		{
			return null;
		}
		string[] array = this.rows;
		int num = this.activeIdx;
		this.activeIdx = num + 1;
		string text = array[num];
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = false;
		for (int i = 0; i < text.Length; i++)
		{
			if (text[i] == '"' && (i == 0 || text[i - 1] != '\\'))
			{
				flag = !flag;
				stringBuilder.Append(text[i]);
			}
			else if ((this.separator == ',' && text[i] == ',' && !flag) || (this.separator != ',' && text[i] == this.separator))
			{
				this.AddToRow(list, stringBuilder.ToString());
				stringBuilder = new StringBuilder();
			}
			else
			{
				char c = text[i];
				if (c == '\u001f')
				{
					c = ',';
				}
				stringBuilder.Append(c);
			}
		}
		if (flag)
		{
			return null;
		}
		this.AddToRow(list, stringBuilder.ToString());
		return list;
	}

	// Token: 0x06000927 RID: 2343 RVA: 0x00035430 File Offset: 0x00033630
	private void AddToRow(List<string> row, string colValue)
	{
		if (colValue.StartsWith("\"") && colValue.EndsWith("\""))
		{
			colValue = colValue.Substring(1, colValue.Length - 2);
		}
		colValue.Replace("\\'", "'");
		row.Add(colValue);
	}

	// Token: 0x06000928 RID: 2344 RVA: 0x00035480 File Offset: 0x00033680
	public int StepActiveRowBack(int count = 1)
	{
		this.activeIdx = IsoMath.IntMax(this.activeIdx - count, 0);
		return this.activeIdx;
	}

	// Token: 0x06000929 RID: 2345 RVA: 0x0003549C File Offset: 0x0003369C
	public List<string> ReadRow(int index)
	{
		int num = this.activeIdx;
		this.activeIdx = index;
		List<string> list = this.ReadRow();
		this.activeIdx = num;
		return list;
	}

	// Token: 0x0600092A RID: 2346 RVA: 0x000354C4 File Offset: 0x000336C4
	public void Dispose()
	{
		this.rows = null;
	}

	// Token: 0x04000D0B RID: 3339
	private string[] rows;

	// Token: 0x04000D0C RID: 3340
	private int activeIdx;

	// Token: 0x04000D0D RID: 3341
	private char separator = ',';
}
