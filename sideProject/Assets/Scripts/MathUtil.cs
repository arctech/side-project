using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MathUtil  {

	public static int RandomSign()  {
    	return Random.value < .5? 1 : -1;
	}
}
