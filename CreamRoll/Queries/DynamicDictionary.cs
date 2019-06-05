using System.Collections.Generic;
using System.Dynamic;

namespace CreamRoll.Queries {
    public class DynamicDictionary : DynamicObject {
        private readonly Dictionary<string, dynamic> dict;

        public DynamicDictionary() {
            dict = new Dictionary<string, dynamic>();
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result) {
            return dict.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value) {
            dict[binder.Name] = value;
            return true;
        }
    }
}