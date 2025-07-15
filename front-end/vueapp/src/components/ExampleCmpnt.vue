<template>
    <div>
        <h5>{{ title }}</h5>
    </div>
</template>

<script>
    export default {
        name: 'ExampleCmpnt',
        data() {
            return {
                title: 'Component Example',
            }
        },
        components: {},
        watch: {},
        methods: {},
        computed: {},
        created() {},
        mounted() {},
        unmounted() {},
    }
</script>

<style scoped>
    /* CSS */
    h5 {
        color: red;
        margin: 200px 0px auto 0px;
        text-align: center;
    }
</style>
