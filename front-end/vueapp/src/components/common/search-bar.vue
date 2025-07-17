<template>
    <div class="row">
        <div class="col">
            <div class="mt-3 mb-2">
                <label for="InputSearch" class="form-label">{{ entity.labelInput }}</label>
                <input type="text" class="form-control" id="InputSearch" ref="searchInpt" :placeholder="entity.placeholderInput" v-model="searchInput" @keydown.enter="search(1, 'search')" @keydown.delete="search(1, 'search')">
            </div>
        </div>
        <template v-if="entity.screen !== 'user' && entity.screen !== 'team'">
            <div class="col-auto content-center" v-if="entity.screen != 'document'">
                <div class="mt-5 mb-2">
                    <a class="btn btn-primary" :title="entity.labelButton" @click="action" v-if="searchInput.length >= 3">{{ upperFormat(entity.labelButton) }}</a>
                    <a class="btn btn-secondary" :title="$t('labelNotAllowed')" v-else>{{ upperFormat(entity.labelButton) }}</a>
                </div>
            </div>
            
            <div class="col-auto content-center" v-else>
                <div class="mt-5 mb-2">
                    <a class="btn btn-primary"
                       :title="$t('labelNewDocument')"
                       @click="action">
                        {{ $t('labelNewDocument') }}
                    </a>
                </div>
            </div>
            </template>
    </div>
</template>

<script>
    export default {
        name: "SearchBar",
        props: {
            entity: {
                required: true,
                type: Object,
                default: {}
            },
            resetInput: {
                required: true,
                type: Boolean,
                default: false
            },
        },
        data() {
            return {
                title: "Component SearchBar",
                searchInput: "",
            }
        },
        components: {},
        watch: {
            resetInput: function (val) {
                this.searchInput = "";
            }
        },
        methods: {
            search: function (page, type) {
                let self = this;
                setTimeout(function () {
                    // Execute every 3 characters pressed or if it is a number greater than zero
                    if (self.searchInput.length > 0 || (!isNaN(self.searchInput) && parseInt(self.searchInput) > 0)) {
                        self.$emit('search', { search: self.searchInput, page: page, type: type });
                    }
                    else {
                        self.$emit('search', { search: "", page: page, type: type });
                    }
                }, 100);
            },
            action: function () {
                this.$emit('action', this.searchInput);
            },
            upperFormat: function (str) {
                return str.toUpperCase();
            },
        },
        computed: {},
        created() { },
        mounted() {
            this.$refs.searchInpt.focus();
        },
        unmounted() { },
    }
</script>

<style scoped>
</style>
