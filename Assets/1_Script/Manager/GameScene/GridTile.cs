using Garage.Props;
using UnityEngine;

namespace Garage
{
	public class GridTile : MonoBehaviour
	{
		private Renderer rend;
		public OwnableProp prop = null;


		void Awake()
		{
			rend = GetComponent<Renderer>();
		}

		public void SetMaterial(Material mat)
		{
			rend.material = mat;
		}

		public bool IsPlaceable(OwnableProp prop)
		{
			if (this.prop == null) return true;
			return this.prop == prop;
		}

		public void SetProp(OwnableProp prop) {
			this.prop = prop;
		}
	}
}