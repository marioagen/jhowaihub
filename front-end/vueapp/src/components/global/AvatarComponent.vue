<template>
    <div
        :class="[
            'rounded-circle',
            'd-flex',
            'align-items-center',
            'justify-content-center',
            `btn-${variant}`,
            'fw-bold',
            'me-3',
            'initials'
        ]"
        :style="{ width: size + 'px', height: size + 'px', fontSize: (size * 0.35) + 'px' }"
    >
        {{ getInitials() }}
    </div>
</template>

<script>
export default {
    name: "AvatarComponent",
    props: {
        name: {
            type: String,
            required: true,
        },
        variant: {
            type: String,
            required: false,
            default: "primary"
        },
        size: {
            type: Number,
            required: false,
            default: 40
        }
    },
    methods: {
        getInitials() {
            if (!this.name) return "";
            const parts = this.name.trim().split(" ");
            if (parts.length === 1) {
                const n = parts[0];
                return (n[0] || "").toUpperCase() + (n[n.length - 1] || "").toUpperCase();
            }
            const first = parts[0][0] || "";
            const last = parts[parts.length - 1].slice(-1) || "";
            return (first + last).toUpperCase();
        }
    }
}
</script>

<style>
.initials {
    width: 40px;
    height: 40px;
    font-size: 14px;
}
</style>
