using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class GUIDUtility
{
	public static string GetUniqueID(bool generateNewIDState = false)
	{
		string uniqueID;

		if (PlayerPrefsUtility.HasKey("guid") && !generateNewIDState)
		{
			uniqueID = PlayerPrefsUtility.GetString("guid");
		}
		else
		{
			uniqueID = GenerateGUID();
			PlayerPrefsUtility.SetString("guid", uniqueID);
		}

        Debug.Log("Unique ID: " + uniqueID);

		return uniqueID;
	}

	public static string GenerateGUID()
	{
		var random = new System.Random();
		DateTime epochStart = new System.DateTime(1970, 1, 1, 8, 0, 0, System.DateTimeKind.Utc);
		double timestamp = (System.DateTime.UtcNow - epochStart).TotalSeconds;

		string uniqueID = String.Format("{0:X}", Convert.ToInt32(timestamp))                    //Time
						+ "-" + String.Format("{0:X}", random.Next(1000000000))                 //Random Number
						+ "-" + String.Format("{0:X}", random.Next(1000000000))                 //Random Number
						+ "-" + String.Format("{0:X}", random.Next(1000000000))                 //Random Number
						+ "-" + String.Format("{0:X}", random.Next(1000000000));                //Random Number

		Debug.Log(uniqueID);

		return uniqueID;
	}

}