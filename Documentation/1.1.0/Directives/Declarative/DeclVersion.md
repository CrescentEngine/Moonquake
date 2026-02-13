# DeclVersion

**DeclVersion** is a directive that specifies which version of the **Moonquake Build Description Language** that this **description file** is targeting. This directive is only callable once and must be called as the first directive (or as the first statement) from the **root description file**. In violation of any of these two cases, the runtime will consider it a fatal error.

---
### Overloads

#### `DeclVersion(Version: String)`

Informs the runtime about which version of **MQBDL** is being targeted.

| Parameter | Type   | Description                                                           |
|-----------|--------|-----------------------------------------------------------------------|
| Version   | String | Version of MQBDL to target, must have the format: Major.Minor[.Patch] |

**Example:**

```plaintext
DeclVersion("1.0.0");
# ...
```
