namespace RevitFabulous.Domain

module ModelQuery =

    type Element = {
        Category: string
        Name: string
    }

    type Category = {
        Name: string
        Elements: Element seq
    }
    