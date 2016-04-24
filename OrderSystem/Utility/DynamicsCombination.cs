using System.Collections.Generic;
using System.Dynamic;

namespace OrderSystem.Utility {
	public class DynamicsCombination {
		public static dynamic CombineDynamics(object item1, object item2) {
			IDictionary<string, object> result = new ExpandoObject();

			foreach(var property in item1.GetType().GetProperties()) {
				if(property.CanRead)
					result[property.Name] = property.GetValue(item1);
			}

			foreach(var property in item2.GetType().GetProperties()) {
				if(property.CanRead)
					result[property.Name] = property.GetValue(item2);
			}

			return result;
		}
	}
}