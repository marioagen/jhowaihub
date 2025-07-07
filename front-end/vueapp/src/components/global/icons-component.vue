<template>
    <img 
        :src="iconPath"
        :title="$t(title)"
        alt=""
    />
</template>

<script>
    export default {
        name: "IconsComponent",
        props: {
            title: {
                type: String,
                required: true, 
            },
            icon: {
                type: String,
                required: true,
            }
        },
        computed: {
            iconPath() {
                return require(`@/assets/icons/${this.icon}.svg`);
            }
        }
    }
</script>