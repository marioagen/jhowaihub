export function buildSelectOptionsWithAll(enumOptions, t, options = {}) {
    const { allValue = null, allLabelKey, labelKeyPrefix } = options;
    const allOption = { value: allValue, label: t(allLabelKey) };
    const rest = (enumOptions || []).map((o) => ({
        value: o.value,
        label:
            t(labelKeyPrefix + o.name) !== labelKeyPrefix + o.name
                ? t(labelKeyPrefix + o.name)
                : o.name,
    }));
    return [allOption, ...rest];
}
